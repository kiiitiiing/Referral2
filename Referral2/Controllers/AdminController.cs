using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Reflection;
using System.Resources;
using System.Globalization;
using System.Threading.Tasks;
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
using Referral2.Resources;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class AdminController : Controller
    {
        private readonly IUserService _userService;
        public readonly ReferralDbContext _context;
        private readonly ResourceManager Roles = new ResourceManager("Referral2.Roles",Assembly.GetExecutingAssembly());
        private readonly ResourceManager Status = new ResourceManager("Referral2.ReferralStatus", Assembly.GetExecutingAssembly());

        public AdminController(ReferralDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }


        public IActionResult AdminDashboard()
        {
            SetCurrentUser();
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();

            var activities = _context.Activity.Where(x=>x.DateReferred.Year.Equals(DateTime.Now.Year));
            var totalDoctors = _context.User.Where(x => x.Level.Equals("doctor")).Count();
            var onlineDoctors = _context.User.Where(x => x.Login.Equals("login")).Count();
            var activeFacility = _context.Facility.Count();
            var referredPatients = _context.Tracking.Where(x => x.DateReferred != default || x.DateAccepted != default || x.DateArrived != default).Count();



            for (int x = 1; x <= 12; x++)
            {
                accepted.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(Status.GetString("ACCEPTED")) || i.Status.Equals(Status.GetString("ARRIVED")) || i.Status.Equals(Status.GetString("ADMITTED")))).Count());
                redirected.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(Status.GetString("REJECTED")) || i.Status.Equals(Status.GetString("TRANSFERRED")))).Count());
            }

            var adminDashboard = new AdminDashboardViewModel(accepted.ToArray(), redirected.ToArray(), totalDoctors, onlineDoctors, activeFacility, referredPatients);

            adminDashboard.Max = accepted.Max() > redirected.Max() ? accepted.Max() : redirected.Max();

            return View(adminDashboard);
        }

        public async Task<IActionResult> SupportUsers()
        {
            var support = _context.User.Where(x => x.Level.Equals(Roles.GetString("SUPPORT")))
                            .Select(x => new SupportUsersViewModel
                            {
                                Id = x.Id,
                                Name = x.Firstname + " " + x.Middlename + " " + x.Lastname,
                                Facility = x.Facility.Name,
                                Contact = x.Contact ?? "N/A",
                                Email = x.Email ?? "N/A",
                                Username = x.Username,
                                Status = x.Status,
                                LastLogin = x.LastLogin == null ? default : x.LastLogin
                            });

            return View(await support.ToListAsync());
        }

        public async Task<IActionResult> Facilities(int? pageNumber)
        {
            var facilities = _context.Facility
                            .Select(x => new FacilitiesViewModel
                            {
                                Id = x.Id,
                                Facility = x.Name,
                                Address = x.Address,
                                Contact = x.Contact,
                                Email = x.Email,
                                Chief = x.ChiefHospital ?? "N/A",
                                Level = x.HospitalLevel,
                                Type = x.HospitalType ?? "N/A"
                            });

            int pageSize = 10;

            return View(await facilities.OrderBy(a => a.Facility).ToListAsync());
            //return View(await PaginatedList<FacilitiesViewModel>.CreateAsync(facilities.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> OnlineUsers(DateTime? setDate)
        {
            var onlineUsers = _context.Login.Where(x => x.Login1.Date.Equals(DateTime.Now.Date) && x.Logout.Equals(default))
                                        .Select(x => new SubOnlineViewModel
                                        {
                                            Facility = x.User.Facility.Name,
                                            Name = x.User.Lastname + ", " + x.User.Firstname + " " + x.User.Middlename,
                                            Level = x.User.Level,
                                            Department = x.User.Department.Description,
                                            Status = x.Status,
                                            Login = x.Login1
                                        });
            var facilityGroup = _context.Login.Where(x => x.Login1.Date.Equals(DateTime.Now.Date) && x.Logout.Equals(default))
                                        .GroupBy(x => x.User.Facility.Name)
                                        .Select(x => new
                                        {
                                            FacilityGroup = x.Key
                                        });

            var onlinePerFacility = facilityGroup
                                        .Select(x => new OnlineUsersAdminViewModel
                                        {
                                            Facility = x.FacilityGroup,
                                            LoggedInUser = onlineUsers.Where(y => y.Facility.Equals(x.FacilityGroup))
                                        });
            return View(await onlinePerFacility.ToListAsync());
        }

        public IActionResult AddSupport()
        {
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x=>x.Name.Contains("")), "Id", "Name");
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> AddSupport([Bind] AddSupportViewModel model)
        {
            if(ModelState.IsValid)
            {
                if(await _userService.RegisterDoctorAsync(model))
                {
                    return RedirectToAction("SupportUsers");
                }

            }
            ViewBag.Facilities = new SelectList(_context.Facility, "Id", "Name");
            return PartialView("",model);
        }



        #region HELPERS
        private void SetCurrentUser()
        {
            if (CurrentUser.user == null)
                CurrentUser.user = _context.User.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        }


        #endregion
    }
}