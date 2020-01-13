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
        public async Task<IActionResult> IncomingReport(int? pageNumber)
        {
            var activities = _context.Activity;
            var tracking = _context.Tracking.Select(t => new IncomingReportViewModel
                {
                    Code = t.Code,
                    Facility = t.ReferredToNavigation.Name,
                    DateAdmitted = activities.FirstOrDefault(x=>x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.ADMITTED)).DateReferred,
                    DateArrived = activities.FirstOrDefault(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.ARRIVED)).DateReferred,
                    DateDischarged = activities.FirstOrDefault(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.DISCHARGED)).DateReferred,
                    DateCancelled = activities.FirstOrDefault(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.CANCELLED)).DateReferred,
                    DateTransferred = activities.FirstOrDefault(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.TRANSFERRED)).DateReferred
                });

            int pageSize = 5;

            ViewBag.Total = tracking.Count();

            return View(await PaginatedList<IncomingReportViewModel>.CreateAsync(tracking.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> OutgoingReport(int? pageNumber)
        {
            var outgoing = _context.Tracking.Select(t => new OutgoingReportViewModel
            {
                Code = t.Code,
                DateReferred = t.DateReferred,
                Seen = t.DateSeen == default || t.DateSeen == null ? default : t.DateReferred - t.DateSeen,
                Accepted = t.DateAccepted == default || t.DateAccepted == null ? default : t.DateReferred - t.DateAccepted,
                Arrived = t.DateArrived == default || t.DateArrived == null ? default : t.DateReferred - t.DateArrived,
                Redirected = default,
                NoAction = t.DateSeen == default || t.DateSeen == null? default : t.DateReferred - t.DateReferred

            });

            int pageSize = 15;
            return View(await PaginatedList<OutgoingReportViewModel>.CreateAsync(outgoing.AsNoTracking(), pageNumber ?? 1, pageSize));
        }
    }
}