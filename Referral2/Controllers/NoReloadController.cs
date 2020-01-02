using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;

namespace Referral2.Controllers
{
    public class NoReloadController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly ResourceManager Roles = new ResourceManager("Referral2.Roles", Assembly.GetExecutingAssembly());
        private readonly ResourceManager Status = new ResourceManager("Referral2.ReferralStatus", Assembly.GetExecutingAssembly());


        public NoReloadController(ReferralDbContext context)
        {
            _context = context;
        }

        public class SelectMuncity
        {
            public int Id { get; set; }
            public string Description { get; set; }
        }



        public class SelectAddressDepartment
        {
            public SelectAddressDepartment(string address, IEnumerable<SelectDepartment> departments)
            {
                Address = address;
                Departments = departments;
            }
            public string Address { get; set; }
            public IEnumerable<SelectDepartment> Departments { get; set; }
        }


        public class SelectDepartment
        {
            public int DepartmentId { get; set; }
            public string DepartmentName { get; set; }
        }

        public class SelectUser
        {
            public int MdId { get; set; }
            public string DoctorName { get; set; }
        }


        [HttpGet]
        public List<SelectMuncity> FilteredBarangay(int? muncityId)
        {
            var filteredBarangay =  _context.Barangay.Where(x => x.MuncityId.Equals(muncityId))
                                                     .Select(y => new SelectMuncity
                                                     {
                                                         Id = y.Id,
                                                         Description = y.Description
                                                     }).ToList();

            return filteredBarangay;
        }


        //  FilterDepartment?facilityId=
        [HttpGet]
        public SelectAddressDepartment FilterDepartment(int? facilityId)
        {
            string address = _context.Facility.Find(facilityId).Address;

            var departments = _context.Department.Select(x => new SelectDepartment
            {
                DepartmentId = x.Id,
                DepartmentName = x.Description
            });

            var faciliyDepartment = _context.User.Where(x => x.FacilityId.Equals(facilityId) && x.Level.Equals(Roles.GetString("DOCTOR")))
                                            .GroupBy(d => d.DepartmentId)
                                            .Select(y => new SelectDepartment
                                            {
                                                DepartmentId = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentId,
                                                DepartmentName = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentName
                                            });

            SelectAddressDepartment selectAddress = new SelectAddressDepartment(address, faciliyDepartment);

            return selectAddress;
        }

        public List<SelectUser> FilterUser(int facilityId, int departmentId)
        {
            var getUser = _context.User.Where(x => x.FacilityId.Equals(facilityId) && x.DepartmentId.Equals(departmentId) && x.Level.Equals(Roles.GetString("DOCTOR")))
                                        .Select(y => new SelectUser
                                        {
                                            MdId = y.Id,
                                            DoctorName = string.IsNullOrEmpty(y.Contact) ? "Dr. " + y.Firstname + " " + y.Middlename + " " + y.Lastname + " - N/A" : "Dr. " + y.Firstname + " " + y.Middlename + " " + y.Lastname + " - " + y.Contact
                                        });

            return getUser.ToList();
        }

        public AdminDashboardViewModel DashboardValues(string level)
        {
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();

            var activities = _context.Activity.Where(x => x.DateReferred.Year.Equals(DateTime.Now.Year));
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


            return adminDashboard;
        }
    }
}