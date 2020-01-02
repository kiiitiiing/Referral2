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

namespace Referral2.Controllers
{
    [Authorize(Policy = "doctor")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ReferralDbContext _context;
        private readonly ResourceManager Status = new ResourceManager("Referral2.ReferralStatus", Assembly.GetExecutingAssembly());

        public HomeController(ILogger<HomeController> logger, ReferralDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            SetCurrentUser();
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();
            var activities = _context.Activity;

            for (int x = 1; x <= 12; x++)
            {
                accepted.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(Status.GetString("ACCEPTED")) || i.Status.Equals(Status.GetString("ARRIVED")) || i.Status.Equals(Status.GetString("ADMITTED")))).Count());
                redirected.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(Status.GetString("REJECTED")) || i.Status.Equals(Status.GetString("TRANSFERRED")))).Count());
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

        private void SetCurrentUser()
        {
            CurrentUser.user = _context.User.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        }

        #endregion
    }
}
