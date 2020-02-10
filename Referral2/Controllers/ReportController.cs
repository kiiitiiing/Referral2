using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Doctor")]
    public class ReportController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        private readonly ICompute _compute;

        public ReportController(ReferralDbContext context, ICompute compute, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _compute = compute;
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
                .Where(x=>x.DateReferred >= StartDate && x.DateReferred <= EndDate)
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
            var departments = _context.Department;
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            ViewBag.Departments = new SelectList(departments, "Id", "Description");
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
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate)
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
            var departments = _context.Department;
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            ViewBag.Departments = new SelectList(departments, "Id", "Description");
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
    }
}