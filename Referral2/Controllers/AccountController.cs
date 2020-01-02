﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Referral2.Data;
using Referral2.Services;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Referral2.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Referral2.Controlers
{
    [Route("account")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        private readonly IConfiguration _configuration;

        private readonly ReferralDbContext _context;

        public AccountController(IUserService userService, IConfiguration configuration, ReferralDbContext context)
        {
            _userService = userService;
            _configuration = configuration;
            _context = context;
        }
        // GET
        [HttpGet]
        [Route("login")]
        public IActionResult Login(string returnUrl)
        {
            if(!User.Identity.IsAuthenticated)
            {
                return View(new LoginViewModel
                {
                    ReturnUrl = returnUrl
                });
            }
            else
            {
                if (User.FindFirstValue(ClaimTypes.Role).Equals("admin"))
                    return RedirectToAction("AdminDashboard", "Admin");
                else if (User.FindFirstValue(ClaimTypes.Role).Equals("doctor"))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("SupportDashboard", "Support");
                }
            }
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if(ModelState.IsValid)
            {
                var (isValid, user) = await _userService.ValidateUserCredentialsAsync(model.Username, model.Password);

                if(isValid)
                {
                    await LoginAsync(user, model);
                    CreateLogin(user.Id);


                    await _context.SaveChangesAsync();
                    if(user.Level.Equals("admin"))
                        return RedirectToAction("AdminDashboard", "Admin");
                    else if (user.Level.Equals("doctor"))
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else if (user.Level.Equals("support"))
                    {
                        return RedirectToAction("SupportDashboard", "Support");
                    }
                }
                ModelState.AddModelError("InvalidCredentials", "Invalid credentials.");
            }

            return View(model);
        }

        [Route("logout")]
        public async Task<IActionResult> Logout(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if(!_configuration.GetValue<bool>("Account:ShowLogoutPrompt"))
            {
                return await Logout();
            }

            return View();
        }
        [Route("accessdenied")]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        public IActionResult Cancel(string returnUrl)
        {
            if(isUrlValid(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Route("logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            if(User.Identity.IsAuthenticated)
            {
                UpdateLogin(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        [Route("profile")]
        public IActionResult Profile()
        {
            var user = CurrentUser.user;

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        #region Helpers
        private void SetCurrentUser(int id)
        {
            if(CurrentUser.user == null)
                CurrentUser.user = _context.User.Find(id);
        }
        private bool isUrlValid(string returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl) && Uri.IsWellFormedUriString(returnUrl, UriKind.Relative);
        }

        private async Task LoginAsync(User user, LoginViewModel model)
        {
            var properties = new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = model.RememberMe
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.Firstname+" "+user.Middlename),
                new Claim(ClaimTypes.Surname, user.Lastname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Contact),
                new Claim(ClaimTypes.Role, user.Level),
                new Claim("Facility", user.FacilityId.ToString()),
                new Claim("Department", user.DepartmentId.ToString()),
                new Claim("Province", user.ProvinceId.ToString()),
                new Claim("Muncity", user.MuncityId.ToString()),
                new Claim("Barangay", user.BarangayId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            SetCurrentUser(user.Id);
            await HttpContext.SignInAsync(principal, properties);
        }
        public void CreateLogin(int userId)
        {
            var newLogin = new Login();

            newLogin.Login1 = DateTime.Now;
            newLogin.Logout = default;
            newLogin.Status = "login";
            newLogin.UserId = userId;
            newLogin.CreatedAt = DateTime.Now;
            newLogin.UpdatedAt = DateTime.Now;
            _context.Add(newLogin);
        }

        public void UpdateLogin(int userId)
        {
            Referral2.Models.Login logout = null;
            try
            {
                logout = _context.Login.First(x => x.Login1.Date == DateTime.Now.Date && x.Logout.Equals(default) && x.UserId.Equals(userId));
                
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            if(logout!=null)
            {
                var currentUser = _context.User.Find(userId);
                logout.Logout = DateTime.Now;
                logout.Status = "logout";
                currentUser.LoginStatus = "logout";
                currentUser.UpdatedAt = DateTime.Now;
                _context.Update(currentUser);
                _context.Update(logout);
                _context.SaveChanges();
            }
        }
        #endregion
    }
}