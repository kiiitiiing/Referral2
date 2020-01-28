using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models.ViewModels;
using Referral2.Models.ViewModels.Mcc;
using Referral2.Models.ViewModels.Support;
using Referral2.Models.ViewModels.ViewPatients;

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
            var totalDoctors = _context.User.Where(x => x.Level.Equals(_roles.Value.DOCTOR) && x.FacilityId.Equals(UserFacility())).Count();
            var onlineDoctors = _context.User.Where(x => x.LoginStatus.Equals("login") && x.Level.Equals(_roles.Value.DOCTOR)).Count();
            var referredPatients = _context.Tracking
                .Where(x => x.DateReferred != default || x.DateAccepted != default || x.DateArrived != default)
                .Where(x=>x.ReferredFrom.Equals(UserFacility())).Count();

            var adminDashboard = new SupportDashboadViewModel(totalDoctors, onlineDoctors, referredPatients);

            return View(adminDashboard);
        }


        // ONLINE USERS PER DEPARTMENT
        public async Task<IActionResult> OnlineUsersDepartment(string dateRange)
        {
            SetDates(dateRange);
            ViewBag.StartDate = StartDate.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.ToString("yyyy/MM/dd");

            var departments = _context.Department;
            var noUser = _context.User.Where(x => x.FacilityId.Equals(UserFacility()));
            var users = await _context.User
                .Where(x => x.FacilityId.Equals(UserFacility()))
                .GroupBy(x => x.DepartmentId)
                .Select(x => new UserDepartmentViewModel
                {
                    Department = departments.Single(y => y.Id.Equals(x.Key)).Description,
                    OnDuty = noUser.Where(y => y.DepartmentId.Equals(x.Key) && y.LoginStatus.Equals("login")).Where(y => y.LastLogin >= StartDate && y.LastLogin <= EndDate).Count(),
                    OffDuty = noUser.Where(y => y.DepartmentId.Equals(x.Key) && y.LoginStatus.Equals("login off")).Where(y => y.LastLogin >= StartDate && y.LastLogin <= EndDate).Count(),
                    NumberOfUsers = noUser.Where(y => y.DepartmentId.Equals(x.Key)).Count()
                })
                .AsNoTracking()
                .ToListAsync();

            return View(users);
        }


        // INCOMING
        public async Task<IActionResult> Incoming(string dateRange)
        {
            SetDates(dateRange);
            ViewBag.StartDate = StartDate.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.ToString("yyyy/MM/dd");

            var trackings = _context.Tracking
                .Where(x => x.ReferredTo.Equals(UserFacility()));

            var acceptCount = trackings.Where(x => x.DateAccepted != default).Count();
            var redirectCount = trackings.Where(x => x.DateTransferred != default).Count();
            var idleCount = 0;
            var noAction = 0;

            var facilities = await _context.Facility
                .Where(x => x.Id != UserFacility() && x.Status.Equals("1"))
                .Select(x => new MccIncomingViewModel
                (
                    x.Name, acceptCount, redirectCount, idleCount, noAction
                ))
                .AsNoTracking()
                .ToListAsync();

            return View(facilities);
        }

        public async Task<IActionResult> TimeFrame(string dateRange)
        {
            return View();

        }

        public IActionResult Track(string code)
        {

            ViewBag.CurrentCode = code;
            var activities = _context.Activity.Where(x => x.Code.Equals(code));
            var feedbacks = _context.Feedback.Where(x => x.Code.Equals(code));

            var track = _context.Tracking
                .Where(x => x.Code.Equals(code))
                .Select(t => new ReferredViewModel
                {
                    PatientName = GlobalFunctions.GetFullName(t.Patient),
                    PatientSex = t.Patient.Sex,
                    PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                    PatientAddress = GlobalFunctions.GetAddress(t.Patient),
                    ReferredBy = t.ReferringMdNavigation == null ? "" : GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                    ReferredTo = t.ActionMdNavigation == null ? "" : GlobalFunctions.GetMDFullName(t.ActionMdNavigation),
                    SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                    CallerCount = activities == null ? 0 : activities.Where(x => x.Status.Equals(_status.Value.CALLING)).Count(),
                    ReCoCount = feedbacks == null ? 0 : feedbacks.Count(),
                    Code = t.Code,
                    Status = t.Status,
                    Activities = activities.OrderByDescending(x => x.CreatedAt),
                    UpdatedAt = t.UpdatedAt
                });

            var tracking = track.FirstOrDefault();

            if (tracking != null)
            {
                ViewBag.Code = tracking.Code;
            }


            return View(tracking);
        }

        public async Task<IActionResult> OnlineDoctors(string name, int? facility)
        {
            ViewBag.CurrentSearch = name;
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.ProvinceId.Equals(UserProvince())), "Id", "Name");
            var onlineUsers = await _context.User.Where(x => x.LoginStatus.Contains("login") && x.LastLogin.Date.Equals(DateTime.Now.Date) && x.FacilityId.Equals(UserFacility())).ToListAsync();

            if (!string.IsNullOrEmpty(name))
            {
                onlineUsers.Where(x => x.Firstname.Contains(name) || x.Middlename.Contains(name) || x.Lastname.Contains(name));
            }
            if (facility != 0)
            {
                onlineUsers.Where(x => x.FacilityId.Equals(facility));
            }

            return View(onlineUsers);
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

        public int UserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        public int UserFacility()
        {
            return int.Parse(User.FindFirstValue("Facility"));
        }
        public int UserDepartment()
        {
            return int.Parse(User.FindFirstValue("Department"));
        }
        public int UserProvince()
        {
            return int.Parse(User.FindFirstValue("Province"));
        }
        public int UserMuncity()
        {
            return int.Parse(User.FindFirstValue("Muncity"));
        }
        public int UserBarangay()
        {
            return int.Parse(User.FindFirstValue("Barangay"));
        }
        public string UserName()
        {
            return "Dr. " + User.FindFirstValue(ClaimTypes.GivenName) + " " + User.FindFirstValue(ClaimTypes.Surname);
        }


        #endregion
    }
}