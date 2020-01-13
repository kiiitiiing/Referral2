﻿using System;
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
using Referral2.Models.ViewModels.Support;
using Referral2.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Referral2.Controllers
{
    [Authorize(Policy = "support")]
    public class SupportController : Controller
    {
        const string SessionDoctorUsername = "_username";
        private readonly ReferralDbContext _context;
        private readonly IUserService _userService;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public SupportController(ReferralDbContext context, IUserService userService, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _userService = userService;
            _roles = roles;
            _status = status;
        }

        public IActionResult SupportDashboard()
        {
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();

            var activities = _context.Activity;
            var totalDoctors = _context.User
                .Where(x => x.Level.Equals(_roles.Value.DOCTOR) && x.FacilityId.Equals(UserFacility())).Count();
            var onlineDoctors = _context.User
                .Where(x => x.Login.Equals("login") && x.FacilityId.Equals(UserFacility())).Count();
            var referredPatients = _context.Tracking
                .Where(x => x.ReferredTo.Equals(UserFacility())).Count();
            var dashboard = new SupportDashboadViewModel(
                totalDoctors,
                onlineDoctors,
                referredPatients);

            return View(dashboard);
        }

        public async Task<IActionResult> ManageUsers(string searchName)
        {
            var doctors = _context.User.Where(x => x.FacilityId.Equals(UserFacility()) && x.Level.Equals(_roles.Value.DOCTOR))
                                       .Select(y => new SupportManageViewModel
                                       {
                                           Id = y.Id,
                                           DoctorName = "Dr. " + y.Firstname + " " + y.Middlename + " " + y.Lastname,
                                           Contact = string.IsNullOrEmpty(y.Contact) ? "N/A" : y.Contact,
                                           DepartmentName = y.Department.Description,
                                           Username = y.Username,
                                           Status = y.Status,
                                           LastLogin = y.LastLogin.Equals(default) ? "Never Login" : y.LastLogin.ToString("MMM dd, yyyy hh:mm tt", System.Globalization.CultureInfo.InvariantCulture)
                                       });

            if(!string.IsNullOrEmpty(searchName))
            {
                doctors = doctors.Where(x => x.DoctorName.Contains(searchName));
            }

            return View(await doctors.ToListAsync());
        }

        public IActionResult AddUser()
        {
            var departments = _context.Department;
            ViewBag.Departments = new SelectList(departments, "Id", "Description");

            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([Bind] AddDoctorViewModel model)
        {
            if(ModelState.IsValid)
            {
                if(await _userService.RegisterSupportAsync(model, UserFacility()))
                {
                    return RedirectToAction("ManageUsers");
                }
            }

            return PartialView(model);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateUser(int? id)
        {
            var departments = _context.Department;

            var currentMd = await _context.User.FindAsync(id);
            if(currentMd != null)
            {
                ViewBag.Statuses = new SelectList(ListContainer.Status, currentMd.Status);
                ViewBag.Departments = new SelectList(departments, "Id", "Description", currentMd.DepartmentId);
                currentMd.Password = "";
            }

            var doctor = returnDoctorInfo(currentMd);
            HttpContext.Session.SetString(SessionDoctorUsername, doctor.Username);

            return PartialView(doctor);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser([Bind] UpdateDoctorViewModel model)
        {
            var doctorLastUsername = HttpContext.Session.GetString(SessionDoctorUsername);
            var departments = _context.Department;
            var doctor = SetDoctorViewModel(model);
            if (ModelState.IsValid)
            {
                if(!string.IsNullOrEmpty(model.Password))
                {
                    if (_userService.ChangePasswordAsync(doctor, model.Password))
                    {
                        return RedirectToAction("ManageUsers", "Support");
                    }
                }
                else
                {
                    if(!_context.User.Any(x => x.Username.Equals(model.Username)))
                    {
                        _context.Update(doctor);
                        await _context.SaveChangesAsync();
                        return RedirectToAction("ManageUsers", "Support");
                    }
                    else
                    {
                        ModelState.AddModelError("Username", "Username already exist");
                    }
                }
                
            }
            ViewBag.Statuses = new SelectList(ListContainer.Status, doctor.Status);
            ViewBag.Departments = new SelectList(departments, "Id", "Description", doctor.DepartmentId);
            return PartialView("~/Views/Support/UpdateUser.cshtml",model);
        }

        

        public IActionResult HospitalInfo()
        {
            var facility = _context.Facility.Find(UserFacility());

            var currentFacility = new HospitalInfoViewModel
            {
                FacilityName = facility.Name,
                Abbreviation = facility.Abbrevation,
                MuncityId = (int)facility.MuncityId,
                BarangayId = (int)facility.BarangayId,
                Address = facility.Address,
                Contact = facility.Contact,
                Email = facility.Email,
                Status = facility.Status
            };
            int provinceFacility = UserProvince();
            int muncityFacility = UserMuncity();
            var muncities = _context.Muncity.Where(x => x.ProvinceId.Equals(provinceFacility));
            var barangays = _context.Barangay.Where(x => x.ProvinceId.Equals(provinceFacility) && x.MuncityId.Equals(muncityFacility));

            ViewBag.Muncities = new SelectList(muncities, "Id", "Description",currentFacility.MuncityId);
            ViewBag.Barangays = new SelectList(barangays, "Id", "Description",currentFacility.BarangayId);
            ViewBag.Statuses = new SelectList(ListContainer.Status);

            return View(currentFacility);
        }

        [HttpPost]
        public async Task<IActionResult> HospitalInfo([Bind] HospitalInfoViewModel model)
        {
            if(ModelState.IsValid)
            {
                var updateFacility = UpdateFacility(model);
                _context.Update(updateFacility);
                await _context.SaveChangesAsync();
            }

            return View(model);
        }

        public async Task<IActionResult> DailyUsers(string date)
        {
            var facilityUsers =await _context.User.Where(x => x.FacilityId.Equals(UserFacility())).AsNoTracking().ToListAsync();


            return View(facilityUsers);
        }





        #region HELPERS
        private User SetDoctorViewModel(UpdateDoctorViewModel model)
        {
            var doctor = _context.User.First(x => x.Username.Equals(model.Username));

            doctor.Firstname = model.Firstname;
            doctor.Middlename = model.Middlename;
            doctor.Lastname = model.Lastname;
            doctor.Contact = model.ContactNumber;
            doctor.Email = model.Email;
            doctor.Designation = model.Designation;
            doctor.DepartmentId = model.Department;
            doctor.Status = model.Status;
            doctor.Level = model.Level;
            doctor.Username = model.Username;
            doctor.UpdatedAt = DateTime.Now;
            return doctor;
        }


        private UpdateDoctorViewModel returnDoctorInfo(User doctor)
        {
            var returnDoctor = new UpdateDoctorViewModel
            {
                Firstname = doctor.Firstname,
                Middlename = doctor.Middlename,
                Lastname = doctor.Lastname,
                ContactNumber = doctor.Contact,
                Email = doctor.Email,
                Designation = doctor.Designation,
                Department = (int)doctor.DepartmentId,
                Level = doctor.Level,
                Username = doctor.Username,
                Status = doctor.Status,
                ConfirmPassword = "",
                Password = ""
            };

            return returnDoctor;
        }
        public Facility UpdateFacility(HospitalInfoViewModel model)
        {
            var facility = _context.Facility.Find(UserFacility());
            facility.Name = model.FacilityName;
            facility.Abbrevation = model.Abbreviation;
            facility.MuncityId = model.MuncityId;
            facility.BarangayId = model.BarangayId;
            facility.Address = model.Address;
            facility.Contact = model.Contact;
            facility.Email = model.Email;
            facility.Status = model.Status;

            return facility;
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