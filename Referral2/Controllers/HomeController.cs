using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Microsoft.Extensions.Options;

namespace Referral2.Controllers
{
    [Authorize(Policy = "doctor")]
    public class HomeController : Controller
    {
        public readonly ReferralDbContext _context;
        private readonly IOptions<ReferralStatus> _status;


        public HomeController( ReferralDbContext context, IOptions<ReferralStatus> status)
        {
            _context = context;
            _status = status;
        }
/*
        public HomeController()
        {

        }*/

        public IActionResult Index()
        {
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();
            var activities = _context.Activity;

            for (int x = 1; x <= 12; x++)
            {
                accepted.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(_status.Value.ACCEPTED) || i.Status.Equals(_status.Value.ARRIVED) || i.Status.Equals(_status.Value.ADMITTED))).Count());
                redirected.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(_status.Value.REJECTED) || i.Status.Equals(_status.Value.TRANSFERRED))).Count());
            }

            DashboardViewModel dashboard = new DashboardViewModel(accepted.ToArray(), redirected.ToArray());

            dashboard.Max = accepted.Max() > redirected.Max() ? accepted.Max() : redirected.Max();

            var test = accepted.ToArray();

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region HELPERS

        #endregion
    }
}
