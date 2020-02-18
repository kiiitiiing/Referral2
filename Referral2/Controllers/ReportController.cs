﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using MoreLinq.Extensions;
using System.Text;
using DinkToPdf.Contracts;
using DinkToPdf;
using System.IO;
using Referral2.Services;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Doctor")]
    public class ReportController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;
        private IConverter _converter;

        private readonly ICompute _compute;

        public ReportController(ReferralDbContext context, IConverter converter, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _converter = converter;
            _roles = roles;
            _status = status;
        }

        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        public async Task<IActionResult> IncomingReport(string daterange, int? page, int? facility, int? department)
        {
            var currentDate = DateTime.Now;
            if(string.IsNullOrEmpty(daterange))
            {
                StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                EndDate = StartDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                StartDate = DateTime.Parse(daterange.Substring(0, daterange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(daterange.Substring(daterange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            int size = 5;

            var activities = _context.Activity
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var tracking = _context.Tracking
                .Where(x => x.ReferredTo == UserFacility && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Select(t => new IncomingReportViewModel
                {
                    ReferredTo = (int)t.ReferredTo,
                    Department = (int)t.DepartmentId,
                    Code = t.Code,
                    Facility = t.ReferredToNavigation.Name,
                    DateAdmitted = activities.First(x=>x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.ADMITTED)).DateReferred,
                    DateArrived = activities.First(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.ARRIVED)).DateReferred,
                    DateDischarged = activities.First(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.DISCHARGED)).DateReferred,
                    DateCancelled = activities.First(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.CANCELLED)).DateReferred,
                    DateTransferred = activities.First(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.TRANSFERRED)).DateReferred
                });

            var facilities = _context.Facility;
            var departments = await AvailableDepartments(UserFacility);
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");
            ViewBag.Total = tracking.Count();


            if (facility != null)
            {
                tracking = tracking.Where(x => x.ReferredTo.Equals(facility));
                ViewBag.Facilities = new SelectList(facilities, "Id", "Name", facility);
                ViewBag.Total = tracking.Count();
            }
            if(department != null)
            {
                tracking = tracking.Where(x => x.Department.Equals(department));
                ViewBag.Departments = new SelectList(departments, "Id", "Description", department);
                ViewBag.Total = tracking.Count();
            }

            return View(await PaginatedList<IncomingReportViewModel>.CreateAsync(tracking.AsNoTracking(), page ?? 1, size));
        }
        // GET OUTGOING REPORT
        public async Task<IActionResult> OutgoingReport(string daterange, int? page, int? facility, int? department)
        {
            var currentDate = DateTime.Now;
            if (string.IsNullOrEmpty(daterange))
            {
                StartDate = new DateTime(currentDate.Year, currentDate.Month, 1);
                EndDate = StartDate.AddMonths(1).AddDays(-1);
            }
            else
            {
                StartDate = DateTime.Parse(daterange.Substring(0, daterange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(daterange.Substring(daterange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            int size = 5;

            var outgoing = _context.Tracking
                .Where(x => x.ReferredFrom == UserFacility && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Select(t => new OutgoingReportViewModel
                {
                    Department = (int)t.DepartmentId,
                    ReferredFrom = (int)t.ReferredFrom,
                    Code = t.Code,
                    DateReferred = t.DateReferred,
                    Seen = t.DateSeen == default ? default : t.DateSeen.Subtract(t.DateReferred).TotalMinutes,
                    Accepted = t.DateAccepted == default ? 0 : t.DateAccepted.Subtract(t.DateReferred).TotalMinutes,
                    Arrived = t.DateArrived == default ? 0 : t.DateArrived.Subtract(t.DateReferred).TotalMinutes,
                    Redirected = t.DateTransferred == default ? 0 : t.DateTransferred.Subtract(t.DateReferred).TotalMinutes,
                    NoAction = t.DateSeen == default ? 0 : t.DateReferred.Subtract(DateTime.Now).TotalMinutes
                });

            var facilities = _context.Facility;
            var departments = await AvailableDepartments(UserFacility);
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            ViewBag.Departments = new SelectList(departments, "DepartmentId", "DepartmentName");
            ViewBag.Total = outgoing.Count();


            if (facility != null)
            {
                outgoing = outgoing.Where(x => x.ReferredFrom.Equals(facility));
                ViewBag.Facilities = new SelectList(facilities, "Id", "Name", facility);
                ViewBag.Total = outgoing.Count();
            }
            if (department != null)
            {
                outgoing = outgoing.Where(x => x.Department.Equals(department));
                ViewBag.Departments = new SelectList(departments, "Id", "Description", department);
                ViewBag.Total = outgoing.Count();
            }

            return View(await PaginatedList<OutgoingReportViewModel>.CreateAsync(outgoing.AsNoTracking(), page ?? 1, size));
        }
        public async Task<IActionResult> NormalFormPdf(string code)
        {
            var patientForm = await _context.PatientForm.SingleOrDefaultAsync(x => x.Code.Equals(code));
            var file = SetPDF(patientForm);
            return File(file, "application/pdf");
        }

        public async Task<IActionResult> PregnantFromPdf(string code)
        {
            var pregnantForm = await _context.PatientForm.SingleOrDefaultAsync(x => x.Code.Equals(code));
            var file = SetPDF(pregnantForm);
            return File(file, "application/pdf");
        }

        #region HELPERS

        public byte[] SetPDF(PatientForm form)
        {
            new CustomAssemblyLoadContext().LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 1.54, Bottom = 1.34, Left = 1.34, Right = 1.34, Unit = Unit.Centimeters },
                DocumentTitle = "Normal patient form"
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = NormalPdf(form),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "site.css") }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            return _converter.Convert(pdf);
        }
        public byte[] SetPDF(PregnantForm form)
        {
            new CustomAssemblyLoadContext().LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 1.54, Bottom = 1.34, Left = 1.34, Right = 1.34, Unit = Unit.Centimeters },
                DocumentTitle = "Normal patient form"
            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                //HtmlContent = NormalPdf(form),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "css", "site.css") }
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            return _converter.Convert(pdf);
        }

        public string NormalPdf(PatientForm form)
        {
            var pdf = new StringBuilder();
            pdf.AppendFormat(@"
                <html>
                    <head></head>
                    <body>
                        <div>
                            <center>    
                                <h2>
                                    CENTRAL VISAYAS HEALTH REFERRAL SYSTEM <br>
                                    <small>
                                        Clinical Referral Form
                                    </small>
                                </h2>
                            </center>
                        </div>
                        <div>
                            <table>
                                <tbody>
                                    <tr>
                                         <br> <br> 
                                        <td colspan = '6'><b>Patient Code:</b><i class='item-color'>{22}</i></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'></td>
                                    </tr>
                                    <tr>
                                         <br> 
                                        <td colspan = '6'><b>Name of Referring Facility:</b><i class='item-color'>{0}</i></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'><b>Facility Contact #:</b><i class='item-color'>{1}</i></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'><b>Address:</b><i class='item-color'>{2}</i></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '3'><b>Referred to:</b><i class='item-color'>{3}</i></td>
                                        <td colspan = '3'><b>Department:</b><i class='item-color'>{4}</i></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'><b>Address:</b><i class='item-color'>{5}</i></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '3'>
                                            <b>Date / Time Referred(ReCo):</b>
                                            <i class='item-color'>
                                                {6}
                                            </i>
                                        </td>
                                        <td colspan = '3'>
                                            <b>Date / Time Transferred:</b>
                                            <i class='item-color'>
                                                {7}
                                            </i>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '3'>
                                            <b>Name of Patient:</b>
                                            <i class='item-color'>
                                                {8}
                                            </i>
                                        </td>
                                        <td>
                                            <b>Age:</b>
                                            <i class='item-color'>
                                                {9}
                                            </i>
                                        </td>
                                        <td>
                                            <b>Sex:</b>
                                            <i class='item-color'>
                                                {10}
                                            </i>
                                        </td>
                                        <td>
                                            <b>Status:</b>
                                            <i class='item-color'>
                                                {11}
                                            </i>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'>
                                            <b>Address:</b>
                                            <i class='item-color'>
                                                {12}
                                            </i>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '3'>
                                            <b>PhilHealth status:</b>
                                            <i class='item-color'>
                                                {13}
                                            </i>
                                        </td>
                                        <td colspan = '3'>
                                            <b>PhilHealth #:</b>
                                            <i class='item-color'>
                                                {14}
                                            </i><br>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan='6'>
                                            <b>Case Summary(pertinent Hx/PE, including meds, labs, course etc.):</b>
                                            <br>
                                            <i class='item-color'>
                                                {15}
                                            </i>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'>
                                            <b>Summary of ReCo(pls.refer to ReCo Guide in Referring Patients Checklist):</b>
                                            <br>
                                            <i class='item-color'>
                                                {16} 
                                            </i> <br> 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'>
                                            <b>Diagnosis / Impression:</b>
                                            <br>
                                            <i class='item-color'>
                                                {17}
                                            </i>
                                             <br> 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'>
                                            <b>Reason for referral:</b>
                                            <br>
                                            <i class='item-color'>
                                                {18}
                                            </i> <br> 
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'></td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'>
                                            <b>Name of referring MD/HCW:</b>
                                            <i class='item-color'>
                                                {19}
                                            </i>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan = '6'>
                                            <b>Contact # of referring MD/HCW:</b>
                                            <i class='item-color'>
                                                {20}
                                            </i>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan='6'>
                                            <b>Name of referred MD/HCW- Mobile Contact # (ReCo):</b>
                                            <i class='item-color'>
                                                {21}
                                            </i>
                                        </td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </body>
                </html>",
                form.ReferringFacility.Name,
                form.ReferringFacility.Contact,
                GlobalFunctions.GetAddress(form.ReferringFacility),
                form.ReferredToNavigation.Name,
                form.Department.Description,
                GlobalFunctions.GetAddress(form.ReferredToNavigation),
                form.TimeReferred.ToString("MMMM d, yyyy h:mm tt",CultureInfo.InvariantCulture),
                form.TimeTransferred == default? "" : form.TimeTransferred.ToString("MMMM d, yyyy h:mm tt", CultureInfo.InvariantCulture),
                GlobalFunctions.GetFullName(form.Patient),
                GlobalFunctions.ComputeAge(form.Patient.DateOfBirth),
                form.Patient.Sex,
                form.Patient.CivilStatus,
                GlobalFunctions.GetAddress(form.Patient),
                form.Patient.PhicStatus,
                form.Patient.PhicId,
                form.CaseSummary,
                form.RecommendSummary,
                form.Diagnosis,
                form.Reason,
                GlobalFunctions.GetMDFullName(form.ReferringMdNavigation),
                form.ReferringMdNavigation.Contact,
                GlobalFunctions.GetMDFullName(form.ReferredMdNavigation),
                form.Code);
            return pdf.ToString();
        }



        private Task<IEnumerable<SelectDepartment>> AvailableDepartments(int facilityId)
        {
            var availableDepartments = _context.User
                .Where(x => x.FacilityId.Equals(facilityId) && x.Level.Equals(_roles.Value.DOCTOR))
                .DistinctBy(x=>x.DepartmentId)
                .Select(x => new SelectDepartment
                {
                    DepartmentId = (int)x.DepartmentId,
                    DepartmentName = x.Department.Description
                });
            return Task.FromResult(availableDepartments);
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