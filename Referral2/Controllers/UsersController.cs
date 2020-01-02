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
    [Authorize(Policy = "doctor")]
    public class UsersController : Controller
    {
        private readonly ReferralDbContext _context;

        private readonly IUserService _userService;

        public UsersController(ReferralDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IActionResult> WhosOnline(string nameSearch, int? facilitySearch)
        {
            ViewBag.CurrentSearch = nameSearch;
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.ProvinceId.Equals(int.Parse(User.FindFirstValue("Province")))),"Id","Name");
            var onlineUsers = await _context.User.Where(x => x.LoginStatus.Equals("login") && x.LastLogin.Date.Equals(DateTime.Now.Date)).ToListAsync();

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
                var (isValid, user) = await _userService.ValidateUserCredentialsAsync(User.FindFirstValue(ClaimTypes.Name), model.CurrentPassword);


                if (isValid)
                {
                    if(await _userService.ChangePasswordAsync(user, model.NewPassword))
                    {
                        ViewData["Status"] = "success";
                    }
                }
            }
            ViewData["Status"] = "failed";

            return PartialView("~/Views/Users/ChangePassword.cshtml",model);
        }
    }
}