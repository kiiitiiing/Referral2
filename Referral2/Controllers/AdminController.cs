using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Referral2.Models.ViewModels.Admin;
using Referral2.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Referral2.Models.ViewModels.Consolidated;
using Referral2.Models.ViewModels.ViewPatients;
using MoreLinq.Extensions;
using System.IO;
using OfficeOpenXml;
using System.Drawing;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        public readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public AdminController(ReferralDbContext context, IUserService userService, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _userService = userService;
            _roles = roles;
            _status = status;
        }


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime Date { get; set; }

        // DASHBOARD
        public IActionResult AdminDashboard()
        {
            var activities = _context.Activity.Where(x=>x.DateReferred.Year.Equals(DateTime.Now.Year));
            var totalDoctors = _context.User.Where(x => x.Level.Equals("doctor")).Count();
            var onlineDoctors = _context.User.Where(x => x.LoginStatus.Contains("login") ).Count();
            var activeFacility = _context.Facility.Count();
            var referredPatients = _context.Tracking.Where(x => x.DateReferred != default || x.DateAccepted != default || x.DateArrived != default).Count();

            var adminDashboard = new AdminDashboardViewModel(totalDoctors, onlineDoctors, activeFacility, referredPatients);

            return View(adminDashboard);
        }

        // GET: ADD FACILITY
        public IActionResult AddFacility()
        {
            var provinces = _context.Province;
            ViewBag.Provinces = new SelectList(provinces, "Id", "Description");
            ViewBag.HospitalLevels = new SelectList(ListContainer.HospitalLevel, "Key", "Value");
            ViewBag.HospitalTypes = new SelectList(ListContainer.HospitalType, "Key", "Value");
            return PartialView();
        }

        // POST: ADD FACILITY
        [HttpPost]
        public async Task<IActionResult> AddFacility([Bind] FacilityViewModel model)
        {
            if (ModelState.IsValid)
            {
                var faciliy = await SetFacilityViewModelAsync(model);
                await _context.AddAsync(faciliy);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (model.Province == null)
                    ModelState.AddModelError("Province", "Please select a province.");
                if (model.Muncity == null)
                    ModelState.AddModelError("Muncity", "Please select a municipality/city.");
                if (model.Barangay == null)
                    ModelState.AddModelError("Barangay", "Please select a barangay.");
                if (model.Level == null)
                    ModelState.AddModelError("Level", "Please select hospital level.");
                if (model.Type == null)
                    ModelState.AddModelError("Type", "Plase select hospital type.");
            }
            var provinces = _context.Province;
            ViewBag.Provinces = new SelectList(provinces, "Id", "Description", model.Province);
            if(model.Muncity != null)
            {
                var muncities = _context.Muncity.Where(x => x.ProvinceId == model.Province);
                ViewBag.Muncities = new SelectList(muncities, "Id", "Description", model.Muncity);
            }
            if(model.Barangay != null)
            {
                var barangays = _context.Barangay.Where(x => x.ProvinceId == model.Province && x.MuncityId == model.Muncity);
                ViewBag.Barangays = new SelectList(barangays, "Id", "Description", model.Barangay);
            }
            ViewBag.HospitalLevels = new SelectList(ListContainer.HospitalLevel, "Key", "Value", model.Level);
            ViewBag.HospitalTypes = new SelectList(ListContainer.HospitalType, "Key", "Value", model.Type);
            return PartialView(model);
        }

        // GET: ADD SUPPORT
        public IActionResult AddSupport()
        {
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.Name.Contains("")), "Id", "Name");
            return PartialView();
        }
        // POST: ADD SUPPORT
        [HttpPost]
        public async Task<IActionResult> AddSupport([Bind] AddSupportViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.RegisterSupportAsync(model))
                {
                    return PartialView("~/Views/Admin/AddSupport");
                }

            }
            ViewBag.Facilities = new SelectList(_context.Facility, "Id", "Name");
            return PartialView("~/Views/Admin/AddSupport", model);
        }

        // CONSOLIDATED
        public async Task<IActionResult> Consolidated(string dateRange, bool export, bool inexport, bool outexport)
        {
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            else
            {
                var dateNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                StartDate = new DateTime(dateNow.Year, 1, 1);
                EndDate = dateNow.AddMonths(1).AddDays(-1);
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

            var consolidated = new List<ConsolidatedViewModel>();

            var activeFacilities = trackings
               .Select(t => t.ReferredTo)
               .Union(
                  trackings
                    .Select(t => t.ReferredFrom)
               );

            foreach(var item in activeFacilities)
            {
                // --------------------- INCOMING ---------------------
                #region INCOMING
                // INCOMING: INCOMING
                var inIncoming = await trackings
                    .Where(x => x.ReferredTo.Equals(item) && x.Status != _status.Value.REJECTED && x.Status != _status.Value.CANCELLED).CountAsync();
                // INCOMING: ACCEPTED
                var inAccepted = await _context.Tracking
                    .Where(x => x.ReferredTo == item && x.DateAccepted != default && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                    .CountAsync();
                // INCOMING: REFERRING FACILITY
                var inReferringFac = await trackings
                    .Where(x => x.ReferredTo == item)
                    .GroupBy(x => x.ReferredFrom)
                    .Select(i => new ListItem
                    {
                        NoItem = i.Count(),
                        ItemName = facilities.SingleOrDefault(x => x.Id == i.Key).Name
                    })
                    .ToListAsync();
                // INCOMING: REFERRING DOCTOS
                var inReferringDoc = await trackings
                    .Where(x => x.ReferredTo == item)
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
                   .Where(p => p.ReferredTo == item)
                   .Select(p => p.Diagnosis)
                   .AsEnumerable()
                   .Concat(
                      pregnantForms
                         .Where(pf => pf.ReferredTo == item)
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
                   .OrderByDescending(x=>x.NoItem)
                   .ToList();

                // INCOMING: REASONS
                var inReason = patientForms
                   .Where(p => p.ReferredTo == item)
                   .Select(p => p.Reason)
                   .AsEnumerable()
                   .Concat(
                      pregnantForms
                         .Where(pf => pf.ReferredTo == item)
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
                // INCOMING: TRANSPORTATIONS
                var inTransportation = await trackings
                    .Where(x => x.ReferredTo == item && !string.IsNullOrEmpty(x.Transportation))
                    .GroupBy(x => x.Transportation)
                    .Select(i => new ListItem
                    {
                        NoItem = i.Count(),
                        ItemName = string.IsNullOrEmpty(i.Key) ? "" : i.Key
                    })
                   .ToListAsync();
                // INCOMING: DEPARTMENT
                var inDepartment = await trackings
                    .Where(x => x.ReferredTo == item)
                    .GroupBy(x => x.DepartmentId)
                    .Select(i => new ListItem
                    {
                        NoItem = i.Count(),
                        ItemName = departments.SingleOrDefault(x => x.Id.Equals(i.Key)).Description
                    })
                   .ToListAsync();

                var inRemarks = await _context.Issue
                    .Where(x => x.Tracking.ReferredTo == item && x.Tracking.DateReferred >= StartDate && x.Tracking.DateReferred <= EndDate)
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
                    .Where(x => x.ReferredFrom.Equals(item) && x.Status != _status.Value.REJECTED && x.Status != _status.Value.CANCELLED).CountAsync();
                // OUTGOING: ACCEPTED
                var outAccepted = await trackings
                    .Where(x => x.ReferredFrom == item && x.DateAccepted != default)
                    .CountAsync();
                // OUTGOING: REDIRECTED
                var outRedirected = await activities
                    .Where(x => x.ReferredFrom.Equals(item) && x.Status.Equals(_status.Value.REJECTED))
                    .CountAsync();
                // OUTGOING: ARCHIVED
                var outArchived = await trackings
                    .Where(x => x.ReferredFrom.Equals(item))
                    .Where(x => x.Status.Equals(_status.Value.REFERRED) || x.Status.Equals(_status.Value.SEEN))
                    .Where(x => DateTime.Now > x.DateReferred.AddDays(3))
                    .CountAsync();
                // OUTGOING: REFERRING FACILITY
                var outReferringFac = await trackings
                    .Where(x => x.ReferredFrom == item)
                    .GroupBy(x => x.ReferredTo)
                    .Select(i => new ListItem
                    {
                        NoItem = i.Count(),
                        ItemName = facilities.SingleOrDefault(x => x.Id == i.Key).Name
                    })
                    .ToListAsync();
                // OUTGOING: REFERRING DOCTOS
                var outReferringDoc = await trackings
                    .Where(x => x.ReferredFrom == item)
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
                   .Where(p => p.ReferringFacilityId == item)
                   .Select(p => p.Diagnosis)
                   .AsEnumerable()
                   .Concat(
                      pregnantForms
                         .Where(pf => pf.ReferringFacility == item)
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
                   .Where(p => p.ReferringFacilityId == item)
                   .Select(p => p.Reason)
                   .AsEnumerable()
                   .Concat(
                      pregnantForms
                         .Where(pf => pf.ReferringFacility == item)
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
                    .Where(x => x.ReferredFrom == item && !string.IsNullOrEmpty(x.Transportation))
                    .GroupBy(x => x.Transportation)
                    .Select(i => new ListItem
                    {
                        NoItem = i.Count(),
                        ItemName = string.IsNullOrEmpty(i.Key) ? "" : i.Key
                    })
                   .ToListAsync();
                // OUTGOING: DEPARTMENT
                var outDepartment = await trackings
                    .Where(x => x.ReferredFrom == item)
                    .GroupBy(x => x.DepartmentId)
                    .Select(i => new ListItem
                    {
                        NoItem = i.Count(),
                        ItemName = departments.SingleOrDefault(x => x.Id.Equals(i.Key)).Description
                    })
                   .ToListAsync();

                var outRemarks = await _context.Issue
                    .Where(x => x.Tracking.ReferredFrom == item && x.Tracking.DateReferred >= StartDate && x.Tracking.DateReferred <= EndDate)
                    .Select(i => new ListItem
                    {
                        ItemName = i.Issue1
                    })
                    .ToListAsync();
                #endregion

                var inAcceptance = trackings
                    .Where(x => x.ReferredTo == item && x.DateAccepted != default)
                    .Select(x => new
                    {
                        mins = x.DateAccepted.Subtract(x.DateReferred).TotalMinutes
                    }).ToList();

                var inHorizontal = await trackings
                    .Where(x => x.ReferredTo.Equals(item) && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                    .Where(x => x.ReferredFromNavigation.HospitalLevel == x.ReferredToNavigation.HospitalLevel)
                    .CountAsync();

                var inVertical = inIncoming - inHorizontal;

                var outHorizontal = await trackings
                    .Where(x => x.ReferredFrom.Equals(item))
                    .Where(x => x.ReferredFromNavigation.HospitalLevel == x.ReferredToNavigation.HospitalLevel)
                    .CountAsync();

                var outVertical = outOutgoing - outHorizontal;

                var inAcc = inAcceptance.Count() > 0 ? inAcceptance.Average(x => x.mins) : 0;

                var inArrival = trackings
                    .Where(x => x.ReferredTo == item && x.DateArrived != default)
                    .Select(x => new
                    {
                        mins = x.DateArrived.Subtract(x.DateReferred).TotalMinutes
                    }).ToList();

                var inArr = inArrival.Count() > 0 ? inArrival.Average(x => x.mins) : 0;

                consolidated.Add(new ConsolidatedViewModel
                {
                    FacilitId = (int)item,
                    FacilityLogo = facilities.Find(item).Picture,
                    FacilityName = facilities.Find(item).Name,
                    InIncoming = inIncoming,
                    InAccepted = inAccepted,
                    InViewed = inIncoming - inAccepted,
                    InReferringFacilities = inReferringFac.OrderByDescending(x => x.NoItem).ToList(),
                    InReferringDoctors = inReferringDoc.OrderByDescending(x=>x.NoItem).ToList(),
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

                });
            }
            
            if(export)
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

        public MemoryStream ExportConsolidated(List<ConsolidatedViewModel> models, string type)
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

                    foreach (var item in models)
                    {
                        row++;
                        var maxRow = row;
                        worksheet.Cells[row, 1].Value = item.FacilityName;  // FACILITY NAME
                        worksheet.Cells[row, 2].Value = item.OutOutgoing.ToString();    // INCOMING
                        worksheet.Cells[row, 3].Value = item.OutViewed.ToString();      // VIEWED ONLY
                        worksheet.Cells[row, 4].Value = item.OutAccepted.ToString();    // ACCEPTED
                        worksheet.Cells[row, 5].Value = underdev;    // ARCHIVED
                        worksheet.Cells[row, 6].Value = underdev;    // REDIRECTED

                        // REFERRED FACILITIES
                        var ctr1 = row;
                        foreach (var items in item.OutReferredFacility)
                        {
                            worksheet.Cells[ctr1, 7].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REFERRED DOCTOR
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.OutReferredDoctor)
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
                        foreach (var items in item.OutDiagnosis)
                        {
                            worksheet.Cells[ctr1, 14].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REASON
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.OutReason)
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
                        foreach (var items in item.OutTransportation)
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
                        foreach (var items in item.OutDepartment)
                        {
                            worksheet.Cells[ctr1, 20].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REMARKS
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.OutRemarks)
                        {
                            worksheet.Cells[ctr1, 21].Value = items.ItemName;
                            ctr1++;
                        }
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        row = maxRow;
                    }
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
                    foreach(var item in models)
                    {
                        row++;
                        var maxRow = row;
                        worksheet.Cells[row, 1].Value = item.FacilityName;  // FACILITY NAME
                        worksheet.Cells[row, 2].Value = item.InIncoming.ToString();    // INCOMING
                        worksheet.Cells[row, 3].Value = item.InViewed.ToString();      // VIEWED ONLY
                        worksheet.Cells[row, 4].Value = item.InAccepted.ToString();    // ACCEPTED
                        // REFERRING FACILITIES
                        var ctr1 = row;
                        foreach (var items in item.InReferringFacilities)
                        {
                            worksheet.Cells[ctr1, 5].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REFERRING DOCTOR
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach(var items in item.InReferringDoctors)
                        {
                            worksheet.Cells[ctr1, 6].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        worksheet.Cells[row, 7].Value = item.InAcceptance.ToString("##.##") +" mins"; //AVERAGE ACCEPTANCE TIME
                        worksheet.Cells[row, 8].Value = item.InArrival.ToString("##.##") + " mins"; //AVERAGE ARRIVAL TIME
                        //DIAGNOSES
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.InDiagnosis)
                        {
                            worksheet.Cells[ctr1, 9].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REASON
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.InReason)
                        {
                            worksheet.Cells[ctr1, 10].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        worksheet.Cells[row, 11].Value = "Under development this column"; // HORIZONTAL REFERRAL
                        worksheet.Cells[row, 12].Value = "Under development this column"; // VERTICAL REFERRAL
                        //TRANSPO
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.InTransportation)
                        {
                            if(!string.IsNullOrEmpty(items.ItemName))
                            {
                                worksheet.Cells[ctr1, 13].Value = ListContainer.TranspoMode[int.Parse(items.ItemName[0].ToString())- 1] + " - " + items.NoItem;
                                ctr1++;
                            }
                        }
                        //DEPARTMENT
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.InDepartment)
                        {
                            worksheet.Cells[ctr1, 14].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REMARKS
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.InRemarks)
                        {
                            worksheet.Cells[ctr1, 15].Value = items.ItemName;
                            ctr1++;
                        }
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        row = maxRow;
                    }
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

                    foreach (var item in models)
                    {
                        row++;
                        var maxRow = row;
                        worksheet.Cells[row, 1].Value = item.FacilityName;  // FACILITY NAME
                        worksheet.Cells[row, 2].Value = item.OutOutgoing.ToString();    // INCOMING
                        worksheet.Cells[row, 3].Value = item.OutViewed.ToString();      // VIEWED ONLY
                        worksheet.Cells[row, 4].Value = item.OutAccepted.ToString();    // ACCEPTED
                        worksheet.Cells[row, 5].Value = underdev;    // ARCHIVED
                        worksheet.Cells[row, 6].Value = underdev;    // REDIRECTED

                        // REFERRED FACILITIES
                        var ctr1 = row;
                        foreach (var items in item.OutReferredFacility)
                        {
                            worksheet.Cells[ctr1, 7].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REFERRED DOCTOR
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.OutReferredDoctor)
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
                        foreach (var items in item.OutDiagnosis)
                        {
                            worksheet.Cells[ctr1, 14].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REASON
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.OutReason)
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
                        foreach (var items in item.OutTransportation)
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
                        foreach (var items in item.OutDepartment)
                        {
                            worksheet.Cells[ctr1, 20].Value = items.ItemName + " - " + items.NoItem;
                            ctr1++;
                        }
                        //REMARKS
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        ctr1 = row;
                        foreach (var items in item.OutRemarks)
                        {
                            worksheet.Cells[ctr1, 21].Value = items.ItemName;
                            ctr1++;
                        }
                        maxRow = ctr1 > maxRow ? ctr1 : maxRow;
                        row = maxRow;
                    }
                }

                worksheet.Cells.AutoFitColumns();
                package.Save();
            }

            stream.Position = 0;

            return stream;
        }

        // DAILY REFERRAL
        public async Task<IActionResult> DailyReferral(int? page, string dateRange, bool export)
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;


            var tracking = _context.Tracking.Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var activity = _context.Activity.Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var rejected = tracking
               .Join(
                  activity,
                  t => t.Code,
                  a => a.Code,
                  (t, a) =>
                     new
                     {
                         Code = t.Code,
                         ReferredTo = t.ReferredTo,
                         Status = a.Status,
                         DateReferred = a.DateReferred
                     });

            var seen = _context.Seen.Where(x => x.Tracking.DateReferred >= StartDate && x.Tracking.DateReferred <= EndDate);


            var facilities = _context.Facility
                .Select(i => new DailyReferralViewModel
                {
                    Facility = i.Name,
                    AcceptedTo = activity.Where(x => x.ReferredTo.Equals(i.Id) && x.DateReferred >= StartDate && x.DateReferred <= EndDate && x.Status.Equals(_status.Value.ACCEPTED)).Count(),
                    RedirectedTo = rejected.Where(x => x.ReferredTo.Equals(i.Id) && x.Status.Equals(_status.Value.REJECTED) && x.DateReferred >= StartDate && x.DateReferred <= EndDate).Select(x=>x.Code).Distinct().Count(),
                    SeenTo = seen.Where(x => x.Tracking.ReferredTo.Equals(i.Id)).Select(x=>x.TrackingId).Distinct().Count(),
                    IncomingTotal = tracking.Where(x=>x.ReferredTo.Equals(i.Id)).Count(),
                    AcceptedFrom = activity.Where(x => x.ReferredFrom.Equals(i.Id) && x.Status.Equals(_status.Value.ACCEPTED) && x.DateReferred >= StartDate && x.DateReferred <= EndDate).Count(),
                    RedirectedFrom = activity.Where(x => x.ReferredFrom.Equals(i.Id) && x.Status.Equals(_status.Value.REJECTED) && x.DateReferred >= StartDate && x.DateReferred <= EndDate).Select(x=>x.Code).Distinct().Count(),
                    SeenFrom = seen.Where(x => x.Tracking.ReferredFrom.Equals(i.Id)).Select(x=>x.TrackingId).Distinct().Count()
                })
                .OrderBy(x => x.Facility);

            if(export)
            {
                string name = $"Daily_Referral-{StartDate.ToString("yyyy-MM-dd")}.xlsx";
                return File(ExportDailyReferral(facilities), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
            }

            int size = 20;

            return View(await PaginatedList<DailyReferralViewModel>.CreateAsync(facilities, page ?? 1, size));
        }

        public MemoryStream ExportDailyReferral(IEnumerable<DailyReferralViewModel> model)
        {
            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                for (int x = 1; x <= 9; x++)
                {
                    if (x <= 7)
                    {
                        worksheet.Cells["A" + x + ":J" + x].Merge = true;
                        worksheet.Cells["A" + x + ":J" + x].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }
                    worksheet.Cells["A" + x + ":J" + x].Style.Font.Bold = true;
                }
                worksheet.Cells["A8:A9"].Merge = true;
                worksheet.Cells["A8:A9"].Value = "Name of Hospital";
                worksheet.Cells["B8:E8"].Merge = true;
                worksheet.Cells["B8:E8"].Value = "Number of Referrals To";
                worksheet.Cells["F8:F9"].Merge = true;
                worksheet.Cells["F8"].Value = "TOTAL";
                worksheet.Cells["F8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["F8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells["G8:I8"].Merge = true;
                worksheet.Cells["G8"].Value = "Number of Referrals From";
                worksheet.Cells["A2"].Value = "Monitoring Tool for Central Visayas Electronic Health Referral System";
                worksheet.Cells["A3"].Value = "Form 2";
                worksheet.Cells["A4"].Value = "DAILY REPORT FOR REFERRALS";
                worksheet.Cells["A6"].Value = "Date: " + StartDate.ToString("MMMM d, yyyy");
                worksheet.Cells["A6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                worksheet.Cells["A8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells["F8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["B8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["I8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["I8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells["B9"].Value = "Accepted";
                worksheet.Cells["C9"].Value = "Redirected";
                worksheet.Cells["D9"].Value = "Seen";
                worksheet.Cells["E9"].Value = "Unseen";
                worksheet.Cells["G9"].Value = "Accepted";
                worksheet.Cells["H9"].Value = "Redirected";
                worksheet.Cells["I9"].Value = "Seen";
                worksheet.Cells["J8:J9"].Merge = true;
                worksheet.Cells["J8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells["J8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["J8"].Value = "TOTAL";

                worksheet.Cells["A10"].LoadFromCollection(model.ToList(), false).AutoFitColumns();
                package.Save();
            }

            stream.Position = 0;

            return stream;
        }


        // DAILY USERS
        public async Task<IActionResult> DailyUsers(int? page, string date)
        {
            Date = DateTime.Now.Date;

            if (!string.IsNullOrEmpty(date))
            {
                Date = DateTime.Parse(date).Date;
            }

            ViewBag.Date = Date;

            var logins = _context.Login.Where(x => x.CreatedAt.Date == Date);
            var users = _context.User;

            var dailyUsers = _context.Facility
                .Select(i => new DailyUsersAdminModel
                {
                    Facility = i.Name,
                    OnDutyHP = logins.Where(x=>x.User.FacilityId == i.Id && x.Login1.Date == Date && x.User.Level == _roles.Value.DOCTOR && x.Status == "login").Select(x=>x.UserId).Distinct().Count(),
                    OffDutyHP = logins.Where(x => x.User.FacilityId == i.Id && x.Login1.Date == Date && x.User.Level == _roles.Value.DOCTOR && x.Status == "login_off").Select(x => x.UserId).Distinct().Count(),
                    OfflineHP = users.Where(x=>x.FacilityId == i.Id && x.Level == _roles.Value.DOCTOR).Count(),
                    OnlineIT = logins.Where(x => x.User.FacilityId == i.Id && x.Login1.Date == Date && x.User.Level == _roles.Value.SUPPORT).Select(x => x.UserId).Distinct().Count(),
                    OfflineIT = users.Where(x => x.FacilityId == i.Id && x.Level == _roles.Value.SUPPORT).Count(),
                })
                .OrderBy(x => x.Facility);

            int size = 20;
            return View(await PaginatedList<DailyUsersAdminModel>.CreateAsync(dailyUsers, page ?? 1, size));
        }

        // EXPORT DAILY USERS
        public async Task<IActionResult> ExportDailyUsers(string date)
        {
            Date = DateTime.Parse(date).Date;
            var users = _context.Login;
            var facilities = await _context.Facility
                .Select(i => new DailyUsersAdminModel
                {
                    Facility = i.Name,
                    OnDutyHP = users.Where(x => x.User.Level.Equals(_roles.Value.DOCTOR) && x.Status.Equals("login") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OffDutyHP = users.Where(x => x.User.Level.Equals(_roles.Value.DOCTOR) && x.Status.Equals("login off") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OfflineHP = users.Where(x => x.User.Level.Equals(_roles.Value.DOCTOR) && x.Status.Equals("logout") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OnlineIT = users.Where(x => x.User.Level.Equals(_roles.Value.SUPPORT) && !x.Status.Equals("logout") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OfflineIT = users.Where(x => x.User.Level.Equals(_roles.Value.SUPPORT) && x.Status.Equals("logout") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                })
                .OrderBy(x => x.Facility)
                .ToListAsync();

            var stream = new MemoryStream();

            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Sheet1");

                for(int x = 1; x <= 9; x++)
                {
                    if(x<=7)
                    {
                        worksheet.Cells["A" + x + ":i" + x].Merge = true;
                        worksheet.Cells["A" + x + ":i" + x].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }
                    worksheet.Cells["A" + x + ":I" + x].Style.Font.Bold = true;
                }
                worksheet.Cells["A8:A9"].Merge = true;
                worksheet.Cells["A8:A9"].Value = "Name of Hospital";
                worksheet.Cells["B8:E8"].Merge = true;
                worksheet.Cells["B8:E8"].Value = "Health Professional";
                worksheet.Cells["F8:H8"].Merge = true;
                worksheet.Cells["F8:H8"].Value = "IT";
                worksheet.Cells["I8:I9"].Merge = true;
                worksheet.Cells["I8:I9"].Value = "TOTAL";
                worksheet.Cells["A8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                worksheet.Cells["A2"].Value = "Monitoring Tool for Central Visayas Electronic Health Referral System";
                worksheet.Cells["A3"].Value = "Form 1";
                worksheet.Cells["A4"].Value = "DAILY REPORT FOR AVAILABLE USERS";
                worksheet.Cells["A6"].Value = "Date: " + Date.ToString("MMMM d, yyyy");
                worksheet.Cells["A6"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Justify;
                worksheet.Cells["A8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["A8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells["F8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["B8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["I8"].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                worksheet.Cells["I8"].Style.VerticalAlignment = OfficeOpenXml.Style.ExcelVerticalAlignment.Center;
                worksheet.Cells["B9"].Value = "On Duty";
                worksheet.Cells["C9"].Value = "Off Duty";
                worksheet.Cells["D9"].Value = "Offline";
                worksheet.Cells["E9"].Value = "Subtotal";
                worksheet.Cells["F9"].Value = "Online";
                worksheet.Cells["G9"].Value = "Offline";
                worksheet.Cells["H9"].Value = "Subtotal";

                worksheet.Cells["A10"].LoadFromCollection(facilities.ToList(), false).AutoFitColumns();
                package.Save();
            }

            stream.Position = 0;
            string name = $"Daily_Users-{Date.ToString("yyyy-MM-dd")}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", name);
        }

        // FACILITIES
        public async Task<IActionResult> Facilities(int? page, string search)
        {
            ViewBag.Filter = search;
            int size = 10;

            var facilities = _context.Facility
                            .Select(x => new FacilitiesViewModel
                            {
                                Id = x.Id,
                                Facility = x.Name,
                                Address = GlobalFunctions.GetAddress(x),
                                Contact = x.Contact,
                                Email = x.Email,
                                Chief = x.ChiefHospital,
                                Level = x.HospitalLevel,
                                Type = x.HospitalType
                            });


            if(!string.IsNullOrEmpty(search))
            {
                facilities = facilities.Where(x => x.Facility.Contains(search));
            }

            return View(await PaginatedList<FacilitiesViewModel>.CreateAsync(facilities.OrderBy(x=>x.Facility), page ?? 1, size));
        }

        // GRAPH
        public async Task<IActionResult> Graph(string year)
        {
            var yearNow = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(year))
            {
                yearNow = int.Parse(year);
            }
            StartDate = new DateTime(yearNow, 1, 1);
            EndDate = new DateTime(yearNow, 12, 31);
            ViewBag.StartDate = StartDate.ToString("MMMM").ToUpper();
            ViewBag.EndDate = EndDate.ToString("MMMM").ToUpper();
            ViewBag.Year = StartDate.ToString("yyyy");
            var trackings = _context.Tracking
                .Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var activities = _context.Activity
                .Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var facilities = await trackings
               .Select(t => t.ReferredTo)
               .Union(
                  trackings
                     .Select(t => t.ReferredFrom)
               )
               .Select(i => new GraphValuesModel
               {
                   Facility = _context.Facility.SingleOrDefault(x=>x.Id == i).Name,
                   Incoming = trackings.Where(x => x.ReferredTo == i).Count(),
                   Accepted = trackings.Where(x => x.ReferredTo == i && x.DateAccepted != default).Count(),
                   Outgoing = trackings.Where(x => x.ReferredFrom == i).Count()
               })
                .AsNoTracking()
                .ToListAsync();
                

            return View(facilities);
        }

        // GET: LOGIN AS
        public ActionResult Login()
        {
            var facilities = _context.Facility.Where(x => x.Status.Equals(1));
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            return View();
        }

        // POST: LOGIN AS
        [HttpPost]
        public async Task<IActionResult> Login(int facility, string level)
        {
            await LoginAsAsync(facility, level);
            if (level.Equals(_roles.Value.ADMIN))
                return RedirectToAction("AdminDashboard", "Admin");
            else if (level.Equals(_roles.Value.DOCTOR))
            {
                return RedirectToAction("Index", "Home");
            }
            else if (level.Equals(_roles.Value.SUPPORT))
            {
                return RedirectToAction("SupportDashboard", "Support");
            }
            else //if (level.Equals(_roles.Value.MCC))
            {
                return RedirectToAction("MccDashboard", "MedicalCenterChief");
            }
        }

        // ONLINE USERS
        public IActionResult OnlineUsers(string date)
        {
            if(!string.IsNullOrEmpty(date))
            {
                StartDate = DateTime.Parse(date);
                EndDate = StartDate.AddDays(1).AddSeconds(1);
            }
            else
            {
                StartDate = DateTime.Now.Date;
                EndDate = StartDate.AddDays(1).AddSeconds(1);
            }

            ViewBag.Date = StartDate.ToString("dd/MM/yyyy");


            var onlineUsers = _context.Login
                .Where(x => x.User.Level != _roles.Value.ADMIN && x.Login1 >= StartDate && x.Login1 <= EndDate)
                .Select(x => new OnlineAdminViewModel
                {
                    UserId = x.UserId,
                    FacilityName = x.User.Facility.Name,
                    UserFullName = GlobalFunctions.FirstToUpper(x.User.Lastname) + ", " + GlobalFunctions.FirstToUpper(x.User.Firstname),
                    UserLevel = GlobalFunctions.FirstToUpper(x.User.Level),
                    UserDepartment = x.User.Department.Description,
                    UserStatus = x.Status,
                    UserLoginTime = x.Login1
                })
                .OrderBy(x=>x.FacilityName)
                .DistinctBy(x => x.UserId);

            return View(onlineUsers);
        }

        // INCOMING PATIENTS
        /*public async Task<IActionResult> PatientIncoming(string dateRange)
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            var tracking = await _context.Tracking
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate && x.ReferredTo == UserFacility)
                .AsNoTracking()
                .ToListAsync();

            return View(tracking);
        }*/

        // REFERRAL STATUS
        public async Task<IActionResult> ReferralStatus(int? page, string dateRange)
        {
            var culture = CultureInfo.InvariantCulture;
            int size = 20;
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            else
            {
                StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                EndDate = StartDate.AddMonths(1).AddDays(-1);
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            var referrals = _context.Tracking
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Select(x => new ReferralStatusViewModel
                {
                    DateReferred = x.DateReferred,
                    FacilityFrom = x.ReferredFromNavigation.Name,
                    FacilityTo = x.ReferredToNavigation.Name,
                    Department = x.Department.Description,
                    PatientName = x.Patient.LastName + ", " + x.Patient.FirstName,
                    Status = x.Status
                })
                .OrderByDescending(x => x.DateReferred);

            return View(await PaginatedList<ReferralStatusViewModel>.CreateAsync(referrals, page ?? 1, size));
        }

        // REMOVE FACILITY
        public void RemoveFacility(int? id)
        {
            var facility = _context.Facility.Find(id);
            _context.Facility.Remove(facility);
            _context.SaveChanges();
        }

        // SUPPORT USERS
        public async Task<IActionResult> SupportUsers(int? page, string search)
        {
            ViewBag.Filter = search;
            int size = 10;

            var support = _context.User.Where(x => x.Level.Equals(_roles.Value.SUPPORT))
                            .Select(x => new SupportUsersViewModel
                            {
                                Id = x.Id,
                                Name = x.Firstname + " " + x.Middlename + " " + x.Lastname,
                                Facility = x.Facility.Name,
                                Contact = x.Contact ?? "N/A",
                                Email = x.Email ?? "N/A",
                                Username = x.Username,
                                Status = x.LoginStatus,
                                LastLogin = x.LastLogin == null ? default : x.LastLogin
                            });

            if (!string.IsNullOrEmpty(search))
                support = support.Where(x => x.Name.Contains(search));

            return View(await PaginatedList<SupportUsersViewModel>.CreateAsync(support.OrderBy(x=>x.Name), page ?? 1, size));
        }

        // GET: UPDATE FACILITY
        [HttpGet]
        public async Task<IActionResult> UpdateFacility(int? id)
        {
            var facilityModel = await _context.Facility.FindAsync(id);

            var facility = await SetFacilityModel(facilityModel);

            var province = _context.Province;
            var muncity = _context.Muncity.Where(x => x.ProvinceId.Equals(facility.Province));
            var barangay = _context.Barangay.Where(x => x.MuncityId.Equals(facility.Muncity));

            ViewBag.Provinces = new SelectList(province, "Id", "Description", facility.Province);
            ViewBag.Muncities = new SelectList(muncity, "Id", "Description", facility.Muncity);
            ViewBag.Barangays = new SelectList(barangay, "Id", "Description", facility.Barangay);
            ViewBag.Levels = new SelectList(ListContainer.HospitalLevel, "Key", "Value", facility.Level);
            ViewBag.Types = new SelectList(ListContainer.HospitalType, "Key", "Value", facility.Type);

            return PartialView("~/Views/Admin/UpdateFacility.cshtml", facility);
        }

        // POST: UPDATE FACILITY
        [HttpPost]
        public async Task<IActionResult> UpdateFacility([Bind] FacilityViewModel model)
        {
            if (string.IsNullOrEmpty(model.Chief))
                model.Chief = "";
            if (ModelState.IsValid)
            {
                var facilities = _context.Facility.Where(x => x.Id != model.Id);
                if (!facilities.Any(x => x.Name.Equals(model.Name)))
                {
                    var saveFacility = await SaveFacilityAsync(model);
                    _context.Update(saveFacility);
                    await _context.SaveChangesAsync();
                    return PartialView("~/Views/Admin/UpdateFacility.cshtml", model);
                }
                else
                {
                    ModelState.AddModelError("Name", "Facility name already exists!");
                }
            }

            var province = _context.Province;
            var muncity = _context.Muncity.Where(x => x.ProvinceId.Equals(model.Province));
            var barangay = _context.Barangay.Where(x => x.MuncityId.Equals(model.Barangay));

            ViewBag.Provinces = new SelectList(province, "Id", "Description", model.Province);
            ViewBag.Muncities = new SelectList(muncity, "Id", "Description", model.Muncity);
            ViewBag.Barangays = new SelectList(barangay, "Id", "Description", model.Barangay);
            ViewBag.Levels = new SelectList(ListContainer.HospitalLevel, "Key", "Value", model.Level);
            ViewBag.Types = new SelectList(ListContainer.HospitalType, "Key", "Value", model.Type);

            return PartialView("~/Views/Admin/UpdateFacility.cshtml", model);
        }

        private async Task<Facility> SaveFacilityAsync(FacilityViewModel model)
        {
            var facility = await _context.Facility.FindAsync(model.Id);
            facility.Name = model.Name;
            facility.Abbrevation = model.Abbrevation;
            facility.ProvinceId = (int)model.Province;
            facility.MuncityId = (int)model.Muncity;
            facility.BarangayId = (int)model.Barangay;
            facility.Address = model.Address;
            facility.Contact = model.Contact;
            facility.Email = model.Email;
            facility.ChiefHospital = model.Chief;
            facility.HospitalLevel = (int)model.Level;
            facility.HospitalType = model.Type;

            return facility;
        }

        // GET: UPDATE SUPPORT
        [HttpGet]
        public async Task<IActionResult> UpdateSupport(int? id)
        {
            var facility = _context.Facility;
            var currentSupport = await _context.User.FindAsync(id);
            if (currentSupport != null)
            {
                ViewBag.Status = new SelectList(ListContainer.UserStatus, "Key", "Value", currentSupport.Status);
                ViewBag.Facility = new SelectList(facility, "Id", "Name", currentSupport.FacilityId);
                currentSupport.Password = "";
            }

            var support = ReturnSupportInfo(currentSupport);
            return PartialView(support);
        }

        // POST: UPDATE SUPPORT
        [HttpPost]
        public async Task<IActionResult> UpdateSupport([Bind] UpdateSupportViewModel model)
        {
            var facilities = _context.Facility;
            var support = await SetSupportViewModel(model);
            if (ModelState.IsValid)
            {
                var supports = _context.User.Where(x => x.Id != support.Id);
                if (!supports.Any(x => x.Username.Equals(model.Username)))
                {
                    if (!string.IsNullOrEmpty(model.Password))
                        _userService.ChangePasswordAsync(support, model.Password);
                    _context.Update(support);
                    await _context.SaveChangesAsync();
                    return PartialView("~/Views/Admin/UpdateSupport.cshtml", model);
                }
                else
                {
                    ModelState.AddModelError("Username", "Username already exist");
                }
            }
            ViewBag.Status = new SelectList(ListContainer.UserStatus, "Key", "Value", support.Status);
            ViewBag.Facility = new SelectList(facilities, "Id", "Description", support.FacilityId);
            return PartialView("~/Views/Admin/UpdateSupport.cshtml", model);
        }

        public async Task<IActionResult> ViewedOnly(int? page, int? facility, string startDate, string endDate, string type)
        {
            StartDate = DateTime.Parse(startDate);
            EndDate = DateTime.Parse(endDate);
            #region Query
            var activities = _context.Activity.Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var feedbacks = _context.Feedback.Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var facilities = _context.Facility.Where(x => x.Id != facility);
            var referred = _context.Tracking
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Where(x => x.Status.Equals(_status.Value.REFERRED) || x.Status.Equals(_status.Value.SEEN))
                .Select(t => new ReferredViewModel
                {
                    PatientId = t.PatientId,
                    PatientName = t.Patient.FirstName + " " + t.Patient.MiddleName + " " + t.Patient.LastName,
                    PatientSex = t.Patient.Sex,
                    PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                    PatientAddress = GlobalFunctions.GetAddress(t.Patient),
                    ReferredBy = GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                    ReferredTo = GlobalFunctions.GetMDFullName(t.ActionMdNavigation),
                    ReferredToId = t.ReferredTo,
                    ReferredFromId = t.ReferredFrom,
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
                });

            if(type.Equals("incoming"))
            {
                referred = referred.Where(x => x.ReferredToId == facility);
            }
            else if(type.Equals("outgoing"))
            {
                referred = referred.Where(x => x.ReferredFromId == facility);
            }
            #endregion

            int size = 10;
            return View(await PaginatedList<ReferredViewModel>.CreateAsync(referred.OrderByDescending(x=>x.UpdatedAt), page ?? 1, size));
        }

        #region HELPERS

        private async Task LoginAsAsync(int facilityId, string level)
        {
            var user = await _context.User.FindAsync(UserId);
            var properties = new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = false
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.Firstname+" "+user.Middlename),
                new Claim(ClaimTypes.Surname, user.Lastname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Contact),
                new Claim(ClaimTypes.Role, level),
                new Claim("Facility", facilityId.ToString()),
                new Claim("Department", user.DepartmentId.ToString()),
                new Claim("Province", user.ProvinceId.ToString()),
                new Claim("Muncity", user.MuncityId.ToString()),
                new Claim("RealRole", user.Level),
                new Claim("RealFacility", user.FacilityId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal, properties);
        }


        private Task<FacilityViewModel> SetFacilityModel(Facility facility)
        {
            var facilityModel = new FacilityViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Abbrevation = facility.Abbrevation,
                Province = facility.ProvinceId,
                Muncity = facility.MuncityId,
                Barangay = facility.BarangayId,
                Address = facility.Address,
                Contact = facility.Contact,
                Email = facility.Email,
                Chief = facility.ChiefHospital,
                Level = facility.HospitalLevel,
                Type = facility.HospitalType
            };

            return Task.FromResult(facilityModel);
        }
        private Task<Facility> SetFacilityViewModelAsync(FacilityViewModel model)
        {
            var facility = new Facility
            {
                Name = model.Name,
                Abbrevation = model.Abbrevation,
                Address = model.Address,
                BarangayId = model.Barangay,
                MuncityId = model.Muncity,
                ProvinceId = (int)model.Province,
                Contact = model.Contact,
                Email = model.Email,
                Status = 1,
                Picture = "",
                ChiefHospital = model.Chief,
                HospitalLevel = (int)model.Level,
                HospitalType = model.Type
            };

            return Task.FromResult(facility);
        }
        private UpdateSupportViewModel ReturnSupportInfo(User currentSupport)
        {
            var support = new UpdateSupportViewModel
            {
                Id = currentSupport.Id,
                Firstname = currentSupport.Firstname,
                Middlename = currentSupport.Middlename,
                Lastname = currentSupport.Lastname,
                ContactNumber = currentSupport.Contact,
                Email = currentSupport.Email,
                Facility = currentSupport.FacilityId,
                Designation = currentSupport.Designation,
                Status = currentSupport.Status,
                Username = currentSupport.Username
            };
            return support;
        }
        private async Task<User> SetSupportViewModel(UpdateSupportViewModel model)
        {
            var support = await _context.User.FindAsync(model.Id);

            support.Firstname = model.Firstname;
            support.Middlename = model.Middlename;
            support.Lastname = model.Lastname;
            support.Contact = model.ContactNumber;
            support.Email = model.Email;
            support.Designation = model.Designation;
            support.FacilityId = model.Facility;
            support.Status = model.Status;
            support.Username = model.Username;
            support.UpdatedAt = DateTime.Now;
            return support;
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