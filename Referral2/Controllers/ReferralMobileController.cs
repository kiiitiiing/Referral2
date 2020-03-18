using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.MobileModels;
using Referral2.Models.ViewModels;
using Referral2.Services;

namespace Referral2.Controllers
{
    public class ReferralMobileController : Controller
    {
        private readonly IUserService _userService;
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public ReferralMobileController(IUserService userService, ReferralDbContext context, IOptions<ReferralStatus> status, IOptions<ReferralRoles> roles)
        {
            _userService = userService;
            _context = context;
            _roles = roles;
            _status = status;
        }

        [HttpGet]
        public async Task<LoginModel> MobileLogin(string username, string password)
        {
            var (isValid, user) = await _userService.ValidateUserCredentialsAsync(username, password);

            var userid = _context.User
                .Where(credentials => credentials.Username.Equals(username))
                .Select(id => id.Id)
                .FirstOrDefault();

            if (isValid)
            {
                var auth = new LoginModel
                {
                    Authenticated = true,
                    UserId = userid,
                };
                return auth;
            }

            var deauth = new LoginModel
            {
                Authenticated = false,
                UserId = 0,
            };

            return deauth;
        }

        [HttpGet]
        public async Task<NotificationModel> Notification(int? id)
        {
            var activity = await _context.Activity
                .FindAsync(id);

            var notification = new NotificationModel
            {
                PatientCode = activity.Code,
                PatientName = GlobalFunctions.GetFullName(activity.Patient),
                ReferringDoctor = GlobalFunctions.GetMDFullName(activity.ActionMdNavigation),
                TrackStatus = activity.Status,
                DisplayNotification = ""
            };

            return notification;
        }

        [HttpGet]
        public async Task<UserModel> GetUser(int? id)
        {
            var getuser = await _context.User.FindAsync(id);

            var departments = _context.Department
                .Where(department => department.Id.Equals(getuser.DepartmentId))
                .Select(column => column.Description)
                .FirstOrDefault();

            var facilities = _context.Facility
                .Where(department => department.Id.Equals(getuser.FacilityId))
                .Select(column => column.Name)
                .FirstOrDefault();

            var user = new UserModel
            {
                Level = getuser.Level,
                UserID = getuser.Id,
                Department = departments,
                Facility = facilities,
                Firstname = getuser.Firstname,
                Middlename = getuser.Middlename,
                Lastname = getuser.Lastname,
            };

            return user;
        }

        public async Task<DashboardViewModel> DashboardValues(int? id)
        {
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();

            IQueryable<Activity> activities = null;

            var getuser = await _context.User.FindAsync(id);

            if (!getuser.Level.Equals(_roles.Value.ADMIN))
                activities = _context.Activity.Where(x => x.DateReferred.Year.Equals(DateTime.Now.Year) && x.ReferredTo.Equals(getuser.FacilityId));
            else
                activities = _context.Activity.Where(x => x.DateReferred.Year.Equals(DateTime.Now.Year));
            for (int x = 1; x <= 12; x++)
            {
                accepted.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(_status.Value.ACCEPTED) || i.Status.Equals(_status.Value.ARRIVED) || i.Status.Equals(_status.Value.ADMITTED))).Count());
                redirected.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(_status.Value.REJECTED) || i.Status.Equals(_status.Value.TRANSFERRED))).Count());
            }
            var adminDashboard = new DashboardViewModel(accepted.ToArray(), redirected.ToArray());
            return adminDashboard;
        }
    }
}