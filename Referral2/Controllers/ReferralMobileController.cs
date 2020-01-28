using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models.ViewModels.MobileModels;
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
        public async Task<bool> MobileLogin(string username, string password)
        {
            var (isValid, user) = await _userService.ValidateUserCredentialsAsync(username, password);

            return isValid;
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
    }
}