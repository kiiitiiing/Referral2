using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Services;

namespace Referral2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReferralMobileController : ControllerBase
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

        public partial class LoginModel
        {
            [Required]
            public string Username { get; set; }
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        public partial class DashboardModel
        {
            public int AverageReferredData { get; set; }
            public int MonthlyReferredData { get; set; }
            public int YearlyReferredData { get; set; }
        }

        public partial class NotificationModel
        {
            public string PatientName { get; set; }
            public string PatientCode { get; set; }
            public string TrackStatus { get; set; }
            public string DisplayNotification { get; set; }
            public string ReferringDoctor { get; set; }
        }

        public async Task<bool> MobileLogin(string username, string password)
        {
            var (isValid, user) = await _userService.ValidateUserCredentialsAsync(username, password);

            return isValid;
        }

        public async Task<DashboardModel> MobileDashboard(int? facilityId)
        {
            return null;
        }

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


        #region HELPERS

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