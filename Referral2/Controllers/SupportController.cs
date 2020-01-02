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
using Referral2.Models.ViewModels.Support;
using Referral2.Services;

namespace Referral2.Controllers
{
    [Authorize(Policy = "support")]
    public class SupportController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IUserService _userService;
        private readonly ResourceManager Status = new ResourceManager("Referral2.ReferralStatus", Assembly.GetExecutingAssembly());
        private readonly ResourceManager Roles = new ResourceManager("Referral2.Roles", Assembly.GetExecutingAssembly());

        public SupportController(ReferralDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public IActionResult SupportDashboard()
        {
            int userFacility = int.Parse(User.FindFirstValue("Facility"));
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();

            var activities = _context.Activity;
            var totalDoctors = _context.User.Where(x => x.Level.Equals("doctor") && x.FacilityId.Equals(userFacility)).Count();
            var onlineDoctors = _context.User.Where(x => x.Login.Equals("login") && x.FacilityId.Equals(userFacility)).Count();
            var referredPatients = _context.Tracking.Where(x => x.DateReferred != default || x.DateAccepted != default || x.DateArrived != default)
                                                    .Where(x => x.ReferredTo.Equals(userFacility)).Count();

            for (int x = 1; x <= 12; x++)
            {
                accepted.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(Status.GetString("ACCEPTED")) || i.Status.Equals(Status.GetString("ARRIVED")) || i.Status.Equals(Status.GetString("ADMITTED")))).Where(x => x.ReferredTo.Equals(userFacility)).Count());
                redirected.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(Status.GetString("REJECTED")) || i.Status.Equals(Status.GetString("TRANSFERRED")))).Where(x => x.ReferredTo.Equals(userFacility)).Count());
            }

            var dashboard = new SupportDashboadViewModel(
                accepted.ToArray(),
                redirected.ToArray(),
                totalDoctors,
                onlineDoctors,
                referredPatients);

            return View(dashboard);
        }

        public async Task<IActionResult> ManageUsers(string searchName)
        {
            var doctors = _context.User.Where(x => x.FacilityId.Equals(int.Parse(User.FindFirstValue("Facility"))) && x.Level.Equals(Roles.GetString("DOCTOR")))
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
                if(await _userService.RegisterSupportAsync(model, int.Parse(User.FindFirstValue("Facility"))))
                {
                    return RedirectToAction("ManageUsers", "Support");
                }
            }

            return PartialView(model);
        }

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

            var doctor = returnDoctorInfo(id);


            return PartialView(doctor);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser([Bind] UpdateDoctorViewModel model)
        {
            if (User != null)
            {
                var doctor = SetDoctorViewModel(model);
                if(await _userService.ChangePasswordAsync(doctor,model.Password))
                {
                    return RedirectToAction("ManageUsers", "Support");
                }
            }
            return PartialView("~/Views/Support/UpdateUser.cshtml",model);
        }

        

        public IActionResult HospitalInfo()
        {
            var facility = _context.Facility.Find(int.Parse(User.FindFirstValue("Facility")));

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
            int provinceFacility = int.Parse(User.FindFirstValue("Province"));
            int muncityFacility = int.Parse(User.FindFirstValue("Muncity"));
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
            int facilityId = int.Parse(User.FindFirstValue("Facility"));
            var facilityUsers =await _context.User.Where(x => x.FacilityId.Equals(facilityId)).AsNoTracking().ToListAsync();


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

            return doctor;
        }


        private UpdateDoctorViewModel returnDoctorInfo(int? id)
        {
            var doctor = _context.User.Find(id);

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
                Status = doctor.Status
            };

            return returnDoctor;
        }
        public Facility UpdateFacility(HospitalInfoViewModel model)
        {
            int facilityId = int.Parse(User.FindFirstValue("Facility"));
            var facility = _context.Facility.Find(facilityId);
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

        #endregion
    }
}