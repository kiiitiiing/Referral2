using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models.ViewModels;
using Referral2.Models.ViewModels.Consolidated;
using Referral2.Models.ViewModels.Mcc;
using Referral2.Models.ViewModels.Support;
using Referral2.Models.ViewModels.ViewPatients;
using Referral2.Models;
using System.Drawing;
using OfficeOpenXml;
using System.IO;
using Referral2.Models.ViewModels.Users;

namespace Referral2.Controllers
{
    [Authorize(Policy = "MCC")]
    public class MedicalCenterChiefController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public MedicalCenterChiefController(ReferralDbContext context, IOptions<ReferralStatus> status, IOptions<ReferralRoles> roles)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }


        // DASHBOARD
        public IActionResult MccDashboard()
        {
            var activities = _context.Activity.Where(x => x.DateReferred.Year.Equals(DateTime.Now.Year));
            var totalDoctors = _context.User.Where(x => x.Level.Equals(_roles.Value.DOCTOR) && x.FacilityId.Equals(UserFacility)).Count();
            var onlineDoctors = _context.User.Where(x => x.LoginStatus.Equals("login") && x.Level.Equals(_roles.Value.DOCTOR) && x.FacilityId == UserFacility && x.LastLogin.Date == DateTime.Now.Date).Count();
            var referredPatients = _context.Tracking
                .Where(x => x.DateReferred != default || x.DateAccepted != default || x.DateArrived != default)
                .Where(x=>x.ReferredFrom.Equals(UserFacility)).Count();

            var adminDashboard = new SupportDashboadViewModel(totalDoctors, onlineDoctors, referredPatients);

            return View(adminDashboard);
        }

        public async Task<IActionResult> ConsolidatedMcc(string dateRange , bool export, bool inexport, bool outexport)
        {
            var dateNow = DateTime.Now;
            StartDate = new DateTime(dateNow.Year, dateNow.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            // TABLES

            var patientForms = _context.PatientForm
                .Where(x => x.TimeReferred >= StartDate && x.TimeReferred <= EndDate);

            var pregnantForms = _context.PregnantForm
                .Where(x => x.ReferredDate >= StartDate && x.ReferredDate <= EndDate);

            var facilities = _context.Facility;
            
            var trackings = _context.Tracking
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);

            var activities = _context.Activity
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);

            var trans = _context.Transportation;

            var departments = _context.Department;

            var doctors = _context.User;


            // --------------------- INCOMING ---------------------
            #region INCOMING
            // INCOMING: INCOMING
            var inIncoming = await trackings
                .Where(x => x.ReferredTo.Equals(UserFacility) && x.Status != _status.Value.REJECTED && x.Status != _status.Value.CANCELLED).CountAsync();
            // INCOMING: ACCEPTED
            var inAccepted = await _context.Tracking
                .Where(x => x.ReferredTo == UserFacility && x.DateAccepted != default && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .CountAsync();
            // INCOMING: REFERRING FACILITY
            var inReferringFac = await trackings
                .Where(x => x.ReferredTo == UserFacility)
                .GroupBy(x => x.ReferredFrom)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = facilities.SingleOrDefault(x => x.Id == i.Key).Name
                })
                .ToListAsync();
            // INCOMING: REFERRING DOCTOS
            var inReferringDoc = await trackings
                .Where(x => x.ReferredTo == UserFacility)
                .GroupBy(x => x.ReferringMd)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = i.Key == null ? "" : GlobalFunctions.GetMDFullName(doctors.SingleOrDefault(x => x.Id == i.Key))
                })
                .Take(10)
                .ToListAsync();


            // INCOMING: DIAGNOSIS

            var inDiagnosis = patientForms
                .Where(p => p.ReferredTo == UserFacility)
                .Select(p => p.Diagnosis)
                .AsEnumerable()
                .Concat(
                    pregnantForms
                        .Where(pf => pf.ReferredTo == UserFacility)
                        .Select(pf => pf.WomanMajorFindings)
                )
                .GroupBy(u => u)
                .Select(
                    x =>
                        new ListItem
                        {
                            NoItem = x.Count(),
                            ItemName = x.Key
                        })
                .OrderByDescending(x => x.NoItem)
                .ToList();

            // INCOMING: REASONS
            var inReason = patientForms
                .Where(p => p.ReferredTo == UserFacility)
                .Select(p => p.Reason)
                .AsEnumerable()
                .Concat(
                    pregnantForms
                        .Where(pf => pf.ReferredTo == UserFacility)
                        .Select(pf => pf.WomanReason)
                )
                .GroupBy(u => u)
                .Select(
                    x =>
                        new ListItem
                        {
                            NoItem = x.Count(),
                            ItemName = x.Key
                        })
                .OrderByDescending(x=>x.NoItem)
                .Take(10)
                .ToList();
            // INCOMING: TRANSPORTATIONS
            var inTransportation = await trackings
                .Where(x => x.ReferredTo == UserFacility && !string.IsNullOrEmpty(x.Transportation))
                .GroupBy(x => x.Transportation)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = string.IsNullOrEmpty(i.Key) ? "" : i.Key
                })
                .ToListAsync();
            // INCOMING: DEPARTMENT
            var inDepartment = await trackings
                .Where(x => x.ReferredTo == UserFacility)
                .GroupBy(x => x.DepartmentId)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = departments.SingleOrDefault(x => x.Id.Equals(i.Key)).Description
                })
                .ToListAsync();

            var inRemarks = await _context.Issue
                .Where(x => x.Tracking.ReferredTo == UserFacility && x.Tracking.DateReferred >= StartDate && x.Tracking.DateReferred <= EndDate)
                .Select(i => new ListItem
                {
                    ItemName = i.Issue1
                })
                .ToListAsync();
            #endregion

            // --------------------- OUTGOING ---------------------
            #region OUTGOING
            // OUTGOING: OUTGOING
            var outOutgoing = await trackings
                .Where(x => x.ReferredFrom.Equals(UserFacility) && x.Status != _status.Value.REJECTED && x.Status != _status.Value.CANCELLED).CountAsync();
            // OUTGOING: ACCEPTED
            var outAccepted = await trackings
                .Where(x => x.ReferredFrom == UserFacility && x.DateAccepted != default)
                .CountAsync();
            // OUTGOING: REDIRECTED
            var outRedirected = await activities
                .Where(x => x.ReferredFrom.Equals(UserFacility) && x.Status.Equals(_status.Value.REJECTED))
                .CountAsync();
            // OUTGOING: ARCHIVED
            var outArchived = await trackings
                .Where(x => x.ReferredFrom.Equals(UserFacility))
                .Where(x => x.Status.Equals(_status.Value.REFERRED) || x.Status.Equals(_status.Value.SEEN))
                .Where(x => DateTime.Now > x.DateReferred.AddDays(3))
                .CountAsync();
            // OUTGOING: REFERRING FACILITY
            var outReferringFac = await trackings
                .Where(x => x.ReferredFrom == UserFacility)
                .GroupBy(x => x.ReferredTo)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = facilities.SingleOrDefault(x => x.Id == i.Key).Name
                })
                .ToListAsync();
            // OUTGOING: REFERRING DOCTOS
            var outReferringDoc = await trackings
                .Where(x => x.ReferredFrom == UserFacility)
                .GroupBy(x => x.ReferringMd)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = i.Key == null ? "" : GlobalFunctions.GetMDFullName(doctors.SingleOrDefault(x => x.Id == i.Key))
                })
                .Take(10)
                .ToListAsync();


            // OUTGOING: DIAGNOSIS

            var outDiagnosis = patientForms
                .Where(p => p.ReferringFacilityId == UserFacility)
                .Select(p => p.Diagnosis)
                .AsEnumerable()
                .Concat(
                    pregnantForms
                        .Where(pf => pf.ReferringFacility == UserFacility)
                        .Select(pf => pf.WomanMajorFindings)
                )
                .GroupBy(u => u)
                .Select(
                    x =>
                        new ListItem
                        {
                            NoItem = x.Count(),
                            ItemName = x.Key
                        })
                .Take(10)
                .ToList();

            // OUTGOING: REASONS
            var outReason = patientForms
                .Where(p => p.ReferringFacilityId == UserFacility)
                .Select(p => p.Reason)
                .AsEnumerable()
                .Concat(
                    pregnantForms
                        .Where(pf => pf.ReferringFacility == UserFacility)
                        .Select(pf => pf.WomanReason)
                )
                .GroupBy(u => u)
                .Select(
                    x =>
                        new ListItem
                        {
                            NoItem = x.Count(),
                            ItemName = x.Key
                        })
                .Take(10)
                .ToList();
            // OUTGOING: TRANSPORTATIONS
            var outTransportation = await trackings
                .Where(x => x.ReferredFrom == UserFacility && !string.IsNullOrEmpty(x.Transportation))
                .GroupBy(x => x.Transportation)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = string.IsNullOrEmpty(i.Key) ? "" : i.Key
                })
                .ToListAsync();
            // OUTGOING: DEPARTMENT
            var outDepartment = await trackings
                .Where(x => x.ReferredFrom == UserFacility)
                .GroupBy(x => x.DepartmentId)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = departments.SingleOrDefault(x => x.Id.Equals(i.Key)).Description
                })
                .ToListAsync();

            var outRemarks = await _context.Issue
                .Where(x => x.Tracking.ReferredFrom == UserFacility && x.Tracking.DateReferred >= StartDate && x.Tracking.DateReferred <= EndDate)
                .Select(i => new ListItem
                {
                    ItemName = i.Issue1
                })
                .ToListAsync();
            #endregion

            var inAcceptance = trackings
                .Where(x => x.ReferredTo == UserFacility && x.DateAccepted != default)
                .Select(x => new
                {
                    mins = x.DateAccepted.Subtract(x.DateReferred).TotalMinutes
                }).ToList();

            var inHorizontal = await trackings
                .Where(x => x.ReferredTo.Equals(UserFacility) && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Where(x => x.ReferredFromNavigation.HospitalLevel == x.ReferredToNavigation.HospitalLevel)
                .CountAsync();

            var inVertical = inIncoming - inHorizontal;

            var outHorizontal = await trackings
                .Where(x => x.ReferredFrom.Equals(UserFacility))
                .Where(x => x.ReferredFromNavigation.HospitalLevel == x.ReferredToNavigation.HospitalLevel)
                .CountAsync();

            var outVertical = outOutgoing - outHorizontal;

            var inAcc = inAcceptance.Count() > 0 ? inAcceptance.Average(x => x.mins) : 0;

            var inArrival = trackings
                .Where(x => x.ReferredTo == UserFacility && x.DateArrived != default)
                .Select(x => new
                {
                    mins = x.DateArrived.Subtract(x.DateReferred).TotalMinutes
                }).ToList();

            var inArr = inArrival.Count() > 0 ? inArrival.Average(x => x.mins) : 0;

            var consolidated = new ConsolidatedViewModel
            {
                FacilitId = (int)UserFacility,
                FacilityLogo = facilities.Find(UserFacility).Picture,
                FacilityName = facilities.Find(UserFacility).Name,
                InIncoming = inIncoming,
                InAccepted = inAccepted,
                InViewed = inIncoming - inAccepted,
                InReferringFacilities = inReferringFac.OrderByDescending(x => x.NoItem).ToList(),
                InReferringDoctors = inReferringDoc.OrderByDescending(x => x.NoItem).ToList(),
                InDiagnosis = inDiagnosis.Take(10).ToList(),
                InReason = inReason.OrderByDescending(x => x.NoItem).ToList(),
                InTransportation = inTransportation.OrderByDescending(x => x.NoItem).ToList(),
                InDepartment = inDepartment.OrderByDescending(x => x.NoItem).ToList(),
                InRemarks = inRemarks.OrderByDescending(x => x.NoItem).ToList(),
                InAcceptance = inAcc,
                InArrival = inArr,
                InHorizontal = inHorizontal,
                InVertical = inVertical,
                OutOutgoing = outOutgoing,
                OutAccepted = outAccepted,
                OutViewed = outOutgoing - outAccepted,
                OutRedirected = outRedirected,
                OutArchived = outArchived,
                OutViewedAcceptance = 0,
                OutViewedRedirection = 0,
                OutAcceptance = 0,
                OutRedirection = 0,
                OutTransport = 0,
                OutHorizontal = outHorizontal,
                OutVertical = outVertical,
                OutReferredFacility = outReferringFac.OrderByDescending(x => x.NoItem).ToList(),
                OutReferredDoctor = outReferringDoc.OrderByDescending(x => x.NoItem).ToList(),
                OutDiagnosis = outDiagnosis.OrderByDescending(x => x.NoItem).ToList(),
                OutReason = outReason.OrderByDescending(x => x.NoItem).ToList(),
                OutTransportation = outTransportation.OrderByDescending(x => x.NoItem).ToList(),
                OutDepartment = outDepartment.OrderByDescending(x => x.NoItem).ToList(),
                OutRemarks = outRemarks.OrderByDescending(x => x.NoItem).ToList()

            };


            if (export)
            {
                string name = $"Consolidated_Report.xlsx";
                return File(ExportConsolidated(consolidated, "all"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
            }
            if (inexport)
            {
                string name = $"Incoming_Report.xlsx";
                return File(ExportConsolidated(consolidated, "incoming"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
            }
            if (outexport)
            {
                string name = $"Outgoing_Report.xlsx";
                return File(ExportConsolidated(consolidated, "outgoing"), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
            }


            return View(consolidated);
        }

        public MemoryStream ExportConsolidated(ConsolidatedViewModel model, string type)
        {
            var stream = new MemoryStream();
            string underdev = "This Column is under development";

            using (var package = new ExcelPackage(stream))
            {
                int row = 1;
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");
                worksheet.Cells.Style.Font.Size = 11;
                worksheet.Cells["A" + row].Value = "Name of Facility";
                worksheet.Cells["C" + row].Value = "Total Viewed Only Referrals";
                worksheet.Cells["D" + row].Value = "Total Accepted Referrals";
                worksheet.Cells["A1:U" + row].Style.Font.Bold = true;
                worksheet.Cells["A1:U" + row].Style.Font.Name = "Arial";

                if (type.Equals("all"))
                {
                    worksheet.Cells["B" + row].Value = "Total Outgoing Referrals";
                    worksheet.Cells["E" + row].Value = "Total Archived Referrals";
                    worksheet.Cells["F" + row].Value = "Total Redirected Referrals";
                    worksheet.Cells["G" + row].Value = "Common Sources(Facility)";
                    worksheet.Cells["H" + row].Value = "Common Referring Doctor HCW/MD (Top 10)";
                    worksheet.Cells["I" + row].Value = "Average Referral Viewed Only Acceptance Turnaround time ";
                    worksheet.Cells["J" + row].Value = "Average Referral Viewed Only Redirection Turnaround time ";
                    worksheet.Cells["K" + row].Value = "Average Referral Acceptance Turnaround time ";
                    worksheet.Cells["L" + row].Value = "Average Referral Redirection Turnaround Time";
                    worksheet.Cells["M" + row].Value = "Average Referral to Transport Turnaround Time";
                    worksheet.Cells["N" + row].Value = "Diagnoses for Outgoing Referral(Top 10)";
                    worksheet.Cells["O" + row].Value = "Reasons for Referral(Top 10)";
                    worksheet.Cells["P" + row].Value = "Reasons for Redirection(Top 10)";
                    worksheet.Cells["Q" + row].Value = "Number of Horizontal referrals";
                    worksheet.Cells["R" + row].Value = "Number of Vertical Referrals";
                    worksheet.Cells["S" + row].Value = "Common Methods of Transportation";
                    worksheet.Cells["T" + row].Value = "Department";
                    worksheet.Cells["U" + row].Value = "Remarks";
                    worksheet.Cells["A" + row + ":U" + row].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A" + row + ":U" + row].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                    
                    row++;
                    var maxRow = row;
                    worksheet.Cells[row, 1].Value = model.FacilityName;  // FACILITY NAME
                    worksheet.Cells[row, 2].Value = model.OutOutgoing.ToString();    // INCOMING
                    worksheet.Cells[row, 3].Value = model.OutViewed.ToString();      // VIEWED ONLY
                    worksheet.Cells[row, 4].Value = model.OutAccepted.ToString();    // ACCEPTED
                    worksheet.Cells[row, 5].Value = underdev;    // ARCHIVED
                    worksheet.Cells[row, 6].Value = underdev;    // REDIRECTED

                    // REFERRED FACILITIES
                    var ctr1 = row;
                    foreach (var items in model.OutReferredFacility)
                    {
                        worksheet.Cells[ctr1, 7].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REFERRED DOCTOR
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutReferredDoctor)
                    {
                        worksheet.Cells[ctr1, 8].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    worksheet.Cells[row, 9].Value = underdev; //AVERAGE ACCEPTANCE TIME
                    worksheet.Cells[row, 10].Value = underdev; //AVERAGE ARRIVAL TIME
                    worksheet.Cells[row, 11].Value = underdev; //AVERAGE ACCEPTANCE TIME
                    worksheet.Cells[row, 12].Value = underdev; //AVERAGE ARRIVAL TIME
                    worksheet.Cells[row, 13].Value = underdev; //AVERAGE ARRIVAL TIME
                                                                //DIAGNOSES
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutDiagnosis)
                    {
                        worksheet.Cells[ctr1, 14].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REASON
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutReason)
                    {
                        worksheet.Cells[ctr1, 15].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    worksheet.Cells[row, 16].Value = underdev; //AVERAGE ACCEPTANCE TIME
                    worksheet.Cells[row, 17].Value = underdev; //AVERAGE ARRIVAL TIME
                    worksheet.Cells[row, 18].Value = underdev; //AVERAGE ARRIVAL TIME
                                                                //TRANSPO
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutTransportation)
                    {
                        if (!string.IsNullOrEmpty(items.ItemName))
                        {
                            worksheet.Cells[ctr1, 19].Value = ListContainer.TranspoMode[int.Parse(items.ItemName[0].ToString()) - 1] + " - " + items.NoItem;
                            ctr1++;
                        }
                    }
                    //DEPARTMENT
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutDepartment)
                    {
                        worksheet.Cells[ctr1, 20].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REMARKS
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutRemarks)
                    {
                        worksheet.Cells[ctr1, 21].Value = items.ItemName;
                        ctr1++;
                    }
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    row = maxRow;
                }
                else if (type.Equals("incoming"))
                {
                    worksheet.Cells["B" + row].Value = "Total Incoming Referrals";
                    worksheet.Cells["E" + row].Value = "Common Sources(Facility)";
                    worksheet.Cells["F" + row].Value = "Common Referring Doctor HCW/MD (Top 10)";
                    worksheet.Cells["G" + row].Value = "Average Referral Acceptance Turnaround time";
                    worksheet.Cells["H" + row].Value = "Average Referral Arrival Turnaround Time";
                    worksheet.Cells["I" + row].Value = "Diagnoses (Top 10)";
                    worksheet.Cells["J" + row].Value = "Reasons (Top 10)";
                    worksheet.Cells["K" + row].Value = "Number of Horizontal referrals";
                    worksheet.Cells["L" + row].Value = "Number of Vertical Referrals";
                    worksheet.Cells["M" + row].Value = "Common Methods of Transportation";
                    worksheet.Cells["N" + row].Value = "Department";
                    worksheet.Cells["O" + row].Value = "Remarks";
                    worksheet.Cells["A" + row + ":O" + row].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A" + row + ":O" + row].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    
                    row++;
                    var maxRow = row;
                    worksheet.Cells[row, 1].Value = model.FacilityName;  // FACILITY NAME
                    worksheet.Cells[row, 2].Value = model.InIncoming.ToString();    // INCOMING
                    worksheet.Cells[row, 3].Value = model.InViewed.ToString();      // VIEWED ONLY
                    worksheet.Cells[row, 4].Value = model.InAccepted.ToString();    // ACCEPTED
                    // REFERRING FACILITIES
                    var ctr1 = row;
                    foreach (var items in model.InReferringFacilities)
                    {
                        worksheet.Cells[ctr1, 5].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REFERRING DOCTOR
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.InReferringDoctors)
                    {
                        worksheet.Cells[ctr1, 6].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    worksheet.Cells[row, 7].Value = model.InAcceptance.ToString("##.##") + " mins"; //AVERAGE ACCEPTANCE TIME
                    worksheet.Cells[row, 8].Value = model.InArrival.ToString("##.##") + " mins"; //AVERAGE ARRIVAL TIME
                    //DIAGNOSES
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.InDiagnosis)
                    {
                        worksheet.Cells[ctr1, 9].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REASON
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.InReason)
                    {
                        worksheet.Cells[ctr1, 10].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    worksheet.Cells[row, 11].Value = "Under development this column"; // HORIZONTAL REFERRAL
                    worksheet.Cells[row, 12].Value = "Under development this column"; // VERTICAL REFERRAL
                    //TRANSPO
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.InTransportation)
                    {
                        if (!string.IsNullOrEmpty(items.ItemName))
                        {
                            worksheet.Cells[ctr1, 13].Value = ListContainer.TranspoMode[int.Parse(items.ItemName[0].ToString()) - 1] + " - " + items.NoItem;
                            ctr1++;
                        }
                    }
                    //DEPARTMENT
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.InDepartment)
                    {
                        worksheet.Cells[ctr1, 14].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REMARKS
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.InRemarks)
                    {
                        worksheet.Cells[ctr1, 15].Value = items.ItemName;
                        ctr1++;
                    }
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    row = maxRow;
                }
                else
                {
                    worksheet.Cells["B" + row].Value = "Total Outgoing Referrals";
                    worksheet.Cells["E" + row].Value = "Total Archived Referrals";
                    worksheet.Cells["F" + row].Value = "Total Redirected Referrals";
                    worksheet.Cells["G" + row].Value = "Common Sources(Facility)";
                    worksheet.Cells["H" + row].Value = "Common Referring Doctor HCW/MD (Top 10)";
                    worksheet.Cells["I" + row].Value = "Average Referral Viewed Only Acceptance Turnaround time ";
                    worksheet.Cells["J" + row].Value = "Average Referral Viewed Only Redirection Turnaround time ";
                    worksheet.Cells["K" + row].Value = "Average Referral Acceptance Turnaround time ";
                    worksheet.Cells["L" + row].Value = "Average Referral Redirection Turnaround Time";
                    worksheet.Cells["M" + row].Value = "Average Referral to Transport Turnaround Time";
                    worksheet.Cells["N" + row].Value = "Diagnoses for Outgoing Referral(Top 10)";
                    worksheet.Cells["O" + row].Value = "Reasons for Referral(Top 10)";
                    worksheet.Cells["P" + row].Value = "Reasons for Redirection(Top 10)";
                    worksheet.Cells["Q" + row].Value = "Number of Horizontal referrals";
                    worksheet.Cells["R" + row].Value = "Number of Vertical Referrals";
                    worksheet.Cells["S" + row].Value = "Common Methods of Transportation";
                    worksheet.Cells["T" + row].Value = "Department";
                    worksheet.Cells["U" + row].Value = "Remarks";
                    worksheet.Cells["A" + row + ":U" + row].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells["A" + row + ":U" + row].Style.Fill.BackgroundColor.SetColor(Color.Yellow);

                    row++;
                    var maxRow = row;
                    worksheet.Cells[row, 1].Value = model.FacilityName;  // FACILITY NAME
                    worksheet.Cells[row, 2].Value = model.OutOutgoing.ToString();    // INCOMING
                    worksheet.Cells[row, 3].Value = model.OutViewed.ToString();      // VIEWED ONLY
                    worksheet.Cells[row, 4].Value = model.OutAccepted.ToString();    // ACCEPTED
                    worksheet.Cells[row, 5].Value = underdev;    // ARCHIVED
                    worksheet.Cells[row, 6].Value = underdev;    // REDIRECTED

                    // REFERRED FACILITIES
                    var ctr1 = row;
                    foreach (var items in model.OutReferredFacility)
                    {
                        worksheet.Cells[ctr1, 7].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REFERRED DOCTOR
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutReferredDoctor)
                    {
                        worksheet.Cells[ctr1, 8].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    worksheet.Cells[row, 9].Value = underdev; //AVERAGE ACCEPTANCE TIME
                    worksheet.Cells[row, 10].Value = underdev; //AVERAGE ARRIVAL TIME
                    worksheet.Cells[row, 11].Value = underdev; //AVERAGE ACCEPTANCE TIME
                    worksheet.Cells[row, 12].Value = underdev; //AVERAGE ARRIVAL TIME
                    worksheet.Cells[row, 13].Value = underdev; //AVERAGE ARRIVAL TIME
                                                                //DIAGNOSES
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutDiagnosis)
                    {
                        worksheet.Cells[ctr1, 14].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REASON
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutReason)
                    {
                        worksheet.Cells[ctr1, 15].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    worksheet.Cells[row, 16].Value = underdev; //AVERAGE ACCEPTANCE TIME
                    worksheet.Cells[row, 17].Value = underdev; //AVERAGE ARRIVAL TIME
                    worksheet.Cells[row, 18].Value = underdev; //AVERAGE ARRIVAL TIME
                                                                //TRANSPO
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutTransportation)
                    {
                        if (!string.IsNullOrEmpty(items.ItemName))
                        {
                            worksheet.Cells[ctr1, 19].Value = ListContainer.TranspoMode[int.Parse(items.ItemName[0].ToString()) - 1] + " - " + items.NoItem;
                            ctr1++;
                        }
                    }
                    //DEPARTMENT
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutDepartment)
                    {
                        worksheet.Cells[ctr1, 20].Value = items.ItemName + " - " + items.NoItem;
                        ctr1++;
                    }
                    //REMARKS
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    ctr1 = row;
                    foreach (var items in model.OutRemarks)
                    {
                        worksheet.Cells[ctr1, 21].Value = items.ItemName;
                        ctr1++;
                    }
                    maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                    row = maxRow;
                }

                worksheet.Cells.AutoFitColumns();
                package.Save();
            }

            stream.Position = 0;

            return stream;
        }

        // ONLINE USERS PER DEPARTMENT
        public async Task<IActionResult> OnlineUsersDepartment(string dateRange)
        {
            SetDates(dateRange);
            ViewBag.StartDate = StartDate.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.ToString("yyyy/MM/dd");

            var departments = _context.Department;
            var noUser = _context.User.Where(x => x.FacilityId.Equals(UserFacility) && x.Level == _roles.Value.DOCTOR);
            var logins = _context.Login.Where(x => x.User.FacilityId == UserFacility && x.User.Level == _roles.Value.DOCTOR && x.Login1 >= StartDate && x.Login1 <= EndDate);
            var users = await _context.User
                .Where(x => x.FacilityId.Equals(UserFacility))
                .Select(x=>x.DepartmentId)
                .Distinct()
                .Select(x => new UserDepartmentViewModel
                {
                    Department = departments.Single(y => y.Id.Equals(x)).Description,
                    OnDuty = logins.Where(c=>c.User.DepartmentId == x && c.Status == "login").Select(x=>x.UserId).Distinct().Count(),
                    OffDuty = logins.Where(c => c.User.DepartmentId == x && c.Status == "login_off").Select(x => x.UserId).Distinct().Count(),
                    NumberOfUsers = noUser.Where(y => y.DepartmentId.Equals(x)).Count()
                })
                .AsNoTracking()
                .ToListAsync();

            return View(users);
        }


        // INCOMING
        public async Task<IActionResult> MccIncoming(int? page, string dateRange)
        {
            var dateNow = DateTime.Now;
            StartDate = new DateTime(dateNow.Year, dateNow.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            var trackings = _context.Tracking
                .Where(x => x.ReferredTo.Equals(UserFacility) && x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var activities = _context.Activity
                .Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);

            var facilities = _context.Facility
                .Select(i => new MccIncomingViewModel
                {
                    Facility = i.Name,
                    AcceptedCount = trackings
                                    .Where(t => t.ReferredFrom == i.Id)
                                    .Join(
                                        activities,
                                        t => t.Code,
                                        a => a.Code,
                                        (t, a) =>
                                            new
                                            {
                                                t = t,
                                                a = a
                                            }
                                    )
                                    .Where(x => x.a.Status == _status.Value.ACCEPTED)
                                    .Count(),
                    RedirectedCount = trackings
                                    .Where(t => t.ReferredFrom == i.Id)
                                    .Join(
                                        activities,
                                        t => t.Code,
                                        a => a.Code,
                                        (t, a) =>
                                            new
                                            {
                                                t = t,
                                                a = a
                                            }
                                    )
                                    .Where(x => x.a.Status == _status.Value.REJECTED)
                                    .Count(),
                    SeenCount = trackings
                                    .Where(t => t.ReferredFrom == i.Id)
                                    .Join(
                                        activities,
                                        t => t.Code,
                                        a => a.Code,
                                        (t, a) =>
                                            new
                                            {
                                                t = t,
                                                a = a
                                            }
                                    )
                                    .Where(x => x.a.Status == _status.Value.SEEN)
                                    .Count(),
                    Total = trackings
                                    .Where(x => x.ReferredFrom == i.Id)
                                    .Count(),
                })
                .AsNoTracking();

            int size = 20;

            return View(await PaginatedList<MccIncomingViewModel>.CreateAsync(facilities.OrderBy(x=>x.Facility), page ?? 1, size));
        }


        public async Task<IActionResult> TimeFrame(string dateRange, int? page)
        {
            var dateNow = DateTime.Now;
            StartDate = new DateTime(dateNow.Year, dateNow.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            var timeFrame = _context.Tracking
                .Where(x => x.ReferredTo == UserFacility && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Select(x => new TimeFrameViewModel
                {
                    Code = x.Code,
                    TimeReferred = x.DateReferred,
                    Seen = x.DateSeen == default ? 0 : x.DateSeen.Subtract(x.DateReferred).TotalMinutes,
                    Accepted = x.DateAccepted == default ? 0 : x.DateAccepted.Subtract(x.DateReferred).TotalMinutes,
                    Arrived = x.DateArrived == default ? 0 : x.DateArrived.Subtract(x.DateReferred).TotalMinutes,
                    Redirected = x.DateTransferred == default ? 0 : x.DateTransferred.Subtract(x.DateReferred).TotalMinutes
                });

            int size = 20;
            return View(await PaginatedList<TimeFrameViewModel>.CreateAsync(timeFrame.OrderByDescending(x=>x.TimeReferred), page ?? 1, size));

        }

        public async Task<IActionResult> Track(string code)
        {
            ViewBag.CurrentCode = code;
            var activities = _context.Activity;
            var feedbacks = _context.Feedback;
            var facilities = _context.Facility.Where(x => x.Id != UserFacility);
            var track = await _context.Tracking
                .Where(x => x.Code.Equals(code))
                .Select(t => new ReferredViewModel
                {
                    PatientId = t.PatientId,
                    PatientName = t.Patient.FirstName + " " + t.Patient.MiddleName + " " + t.Patient.LastName,
                    PatientSex = t.Patient.Sex,
                    PatientAge = t.Patient.DateOfBirth.ComputeAge(),
                    PatientAddress = GlobalFunctions.GetAddress(t.Patient),
                    ReferredBy = GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                    ReferredTo = GlobalFunctions.GetMDFullName(t.ActionMdNavigation),
                    ReferredToId = t.ReferredTo,
                    TrackingId = t.Id,
                    SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                    CallerCount = activities.Where(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.CALLING)).Count(),
                    IssueCount = _context.Issue.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                    ReCoCount = feedbacks.Where(x => x.Code.Equals(t.Code)).Count(),
                    Travel = string.IsNullOrEmpty(t.Transportation),
                    Code = t.Code,
                    Status = t.Status,
                    Pregnant = t.Type.Equals("pregnant"),
                    Seen = t.DateSeen != default,
                    Walkin = t.WalkIn.Equals("yes"),
                    UpdatedAt = t.UpdatedAt,
                    Activities = activities.Where(x => x.Code.Equals(t.Code)).OrderByDescending(x => x.CreatedAt)
                        .Select(i => new ActivityLess
                        {
                            Status = i.Status,
                            DateAction = i.DateReferred.ToString("MMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture),
                            FacilityFrom = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Name,
                            FacilityFromContact = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Contact,
                            FacilityTo = t.ReferredToNavigation.Name,
                            PatientName = i.Patient.FirstName + " " + i.Patient.MiddleName + " " + i.Patient.LastName,
                            ActionMd = GlobalFunctions.GetMDFullName(i.ActionMdNavigation),
                            ReferringMd = GlobalFunctions.GetMDFullName(i.ReferringMdNavigation),
                            Remarks = i.Remarks
                        })
                })
                .FirstOrDefaultAsync();


            if (track != null)
            {
                ViewBag.Code = track.Code;
            }


            return View(track);
        }

        public async Task<IActionResult> OnlineDoctors(string nameSearch, int? facilitySearch)
        {
            ViewBag.CurrentSearch = nameSearch;
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.ProvinceId.Equals(UserProvince)), "Id", "Name");
            var logins = _context.Login.Where(x => x.Login1.Date.Equals(DateTime.Now.Date));
            var onlineUsers = await _context.User
                .Where(x => x.LoginStatus.Contains("login") && x.Level.Equals(_roles.Value.DOCTOR) && x.LastLogin.Date.Equals(DateTime.Now.Date) && x.FacilityId.Equals(UserFacility))
                .Select(x => new WhosOnlineModel
                {
                    DoctorName = GlobalFunctions.GetMDFullName(x),
                    FacilityAbrv = x.Facility.Abbrevation,
                    Contact = x.Contact,
                    Department = x.Department.Description,
                    LoginStatus = logins.Where(i => i.UserId.Equals(x.Id)).OrderByDescending(i => i.Login1).First().Status.Equals("login"),
                    LoginTime = logins.Where(i => i.UserId.Equals(x.Id)).OrderByDescending(i => i.Login1).First().Login1
                })
                .ToListAsync();

            if (!string.IsNullOrEmpty(nameSearch))
            {
                onlineUsers.Where(x => x.DoctorName.Contains(nameSearch));
            }
            if (facilitySearch != 0)
            {
                onlineUsers.Where(x => x.FacilityId.Equals(facilitySearch));
            }

            return View(onlineUsers);
        }

        public async Task<IActionResult> OnlineMcc()
        {
            var OnlineMccFacility = await _context.Facility
                .Select(x => new OnlineMccModel
                {
                    FacilityId = x.Id,
                    FacilityName = x.Name,
                    Online = x.User.Where(c => c.FacilityId == x.Id && c.Level == _roles.Value.MCC && c.LoginStatus.Contains("login") && c.LastLogin.Date == DateTime.Now.Date).FirstOrDefault().Equals(null)
                })
                .OrderBy(x=>x.FacilityName)
                .ToListAsync();

            return PartialView(OnlineMccFacility);
        }

        #region HELPERS

        public void SetDates(string dateRange)
        {
            var lastDate = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, lastDate);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
        }

        public int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        public int UserFacility => int.Parse(User.FindFirstValue("Facility"));
        public int UserDepartment => int.Parse(User.FindFirstValue("Department"));
        public int UserProvince => int.Parse(User.FindFirstValue("Province"));
        public int UserMuncity => int.Parse(User.FindFirstValue("Muncity"));
        public int UserBarangay => int.Parse(User.FindFirstValue("Barangay"));
        public string UserName => "Dr. " + User.FindFirstValue(ClaimTypes.GivenName) + " " + User.FindFirstValue(ClaimTypes.Surname);


        #endregion
    }
}