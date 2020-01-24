using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Referral2.Data;
using Referral2.Models;
using Referral2.Helpers;
using Referral2.Models.ViewModels.Users;
using Microsoft.EntityFrameworkCore;
using Referral2.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace Referral2.Controllers
{
    public class UsersController : Controller
    {
        private readonly ReferralDbContext _context;

        private readonly IUserService _userService;

        public UsersController(ReferralDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        [Authorize(Policy = "doctor")]
        public async Task<IActionResult> WhosOnline(string nameSearch, int? facilitySearch)
        {
            ViewBag.CurrentSearch = nameSearch;
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.ProvinceId.Equals(UserProvince())),"Id","Name");
            var onlineUsers = await _context.User.Where(x => x.LoginStatus.Contains("login") && x.LastLogin.Date.Equals(DateTime.Now.Date) && x.FacilityId.Equals(UserFacility())).ToListAsync();

            if(!string.IsNullOrEmpty(nameSearch))
            {
                onlineUsers.Where(x => x.Firstname.Contains(nameSearch) || x.Middlename.Contains(nameSearch) || x.Lastname.Contains(nameSearch));
            }
            if(facilitySearch != 0)
            {
                onlineUsers.Where(x => x.FacilityId.Equals(facilitySearch));
            }

            return View(onlineUsers);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["Status"] = "sumitng";
            return PartialView("~/Views/Users/ChangePassword.cshtml");
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword([Bind] ChangePasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var (isValid, user) = await _userService.ValidateUserCredentialsAsync(UserUsername(), model.CurrentPassword);


                if (isValid)
                {
                    if( _userService.ChangePasswordAsync(user, model.NewPassword))
                    {
                        ViewData["Status"] = "success";
                    }
                }
                else
                {
                    ModelState.AddModelError("CurrentPassword", "Wrong Password");
                }
            }
            ViewData["Status"] = "failed";

            return PartialView("~/Views/Users/ChangePassword.cshtml", model);
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

        public string UserUsername()
        {
            return User.FindFirstValue(ClaimTypes.Name);
        }


        #endregion
    }
}