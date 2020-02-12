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
using System.Security.Claims;
using Referral2.Models.ViewModels.ViewPatients;
using Microsoft.Extensions.Options;

namespace Referral2.Controllers
{
    public class NoReloadController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;


        public NoReloadController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        public partial class SelectAddress
        {
            public int Id { get; set; }
            public string Description { get; set; }
        }

        public partial class SelectAddressDepartment
        {
            public SelectAddressDepartment(string address, IEnumerable<SelectDepartment> departments)
            {
                Address = address;
                Departments = departments;
            }
            public string Address { get; set; }
            public IEnumerable<SelectDepartment> Departments { get; set; }
        }

        public partial class SelectUser
        {
            public int MdId { get; set; }
            public string DoctorName { get; set; }
        }

        [HttpGet]
        public int NumberNotif()
        {
            var incoming = from t in _context.Tracking.Where(f => f.ReferredTo.Equals(UserFacility))
                           join a in _context.Activity
                           on t.Code equals a.Code
                           into tact
                           from c in tact.DefaultIfEmpty()
                           select new IncomingViewModel()
                           {
                               ReferredToId = (int)t.ReferredTo,
                               DateAction = c.DateReferred
                           };
            incoming = incoming.Where(x => x.ReferredToId.Equals(UserFacility) && x.DateAction.Date.Equals(DateTime.Now.Date));

            return incoming.Count();
        }

        public List<SelectDepartment> AvailableDepartments(int facilityId)
        {
            var departments = _context.Department
                 .Select(x => new SelectDepartment
                 {
                     DepartmentId = x.Id,
                     DepartmentName = x.Description
                 });
            var availableDepartments = _context.User
                .Where(x => x.FacilityId.Equals(facilityId) && x.Level.Equals(_roles.Value.DOCTOR))
                .GroupBy(d => d.DepartmentId)
                .Select(y => new SelectDepartment
                {
                    DepartmentId = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentId,
                    DepartmentName = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentName
                });
            return availableDepartments.ToList();
        }

        [HttpGet]
        public List<SelectAddress> FilteredBarangay(int? muncityId)
        {
            var filteredBarangay =  _context.Barangay.Where(x => x.MuncityId.Equals(muncityId))
                                                     .Select(y => new SelectAddress
                                                     {
                                                         Id = y.Id,
                                                         Description = y.Description
                                                     }).ToList();

            return filteredBarangay;
        }
        
        [HttpGet]
        public void ChangeLoginStatus(string status)
        {
            var currentUser = _context.User.Find(UserId);
            var login = new Login
            {
                UserId = UserId,
                Login1 = DateTime.Now,
                Status = status == "onDuty" ? "login" : "login off",
                Logout = default,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            currentUser.LoginStatus = status == "onDuty" ? "login" : "login off";
            currentUser.UpdatedAt = DateTime.Now;
            _context.Add(login);
            _context.Update(currentUser);
            _context.SaveChanges();
        }

        //  FilterDepartment?facilityId=
        [HttpGet]
        public SelectAddressDepartment FilterDepartment(int? facilityId)
        {
            var facility = _context.Facility.Find(facilityId);
            if (facility == null)
                return null;
            string facilityAddress = facility.Address.Equals("none") ? "" : facility.Address + ", ";
            string barangay = facility.Barangay == null ? "" : facility.Barangay.Description + ", ";
            string muncity = facility.Muncity == null ? "" : facility.Muncity.Description + ", ";
            string province = facility.Province == null ? "" : facility.Province.Description;
            string address = facilityAddress + barangay + muncity + province;

            var departments = _context.Department.Select(x => new SelectDepartment
            {
                DepartmentId = x.Id,
                DepartmentName = x.Description
            });

            var faciliyDepartment = _context.User.Where(x => x.FacilityId.Equals(facilityId) && x.Level.Equals(_roles.Value.DOCTOR))
                                            .GroupBy(d => d.DepartmentId)
                                            .Select(y => new SelectDepartment
                                            {
                                                DepartmentId = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentId,
                                                DepartmentName = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentName
                                            });

            SelectAddressDepartment selectAddress = new SelectAddressDepartment(address, faciliyDepartment);

            return selectAddress;
        }

        public async Task<string> GetFaciliyAddress(int? id)
        {
            var facility = await _context.Facility
                .FindAsync(id);
            var address = GlobalFunctions.GetAddress(facility);
            return address;
        }

        public async Task<List<SelectAddress>> GetMuncities(int? id)
        {
            var muncities = await _context.Muncity
                .Where(x => x.ProvinceId.Equals(id))
                .Select(x => new SelectAddress
                {
                    Id = x.Id,
                    Description = x.Description
                })
                .ToListAsync();

            return muncities;
        }

        public async Task<List<SelectAddress>> GetBarangays(int? id)
        {
            var barangays = await _context.Barangay
                .Where(x => x.MuncityId.Equals(id))
                .Select(x => new SelectAddress
                {
                    Id = x.Id,
                    Description = x.Description
                })
                .ToListAsync();

            return barangays;
        }

        public List<SelectUser> FilterUser(int facilityId, int departmentId)
        {
            var getUser = _context.User.Where(x => x.FacilityId.Equals(facilityId) && x.DepartmentId.Equals(departmentId) && x.Level.Equals(_roles.Value.DOCTOR))
                                        .Select(y => new SelectUser
                                        {
                                            MdId = y.Id,
                                            DoctorName = string.IsNullOrEmpty(y.Contact) ? "Dr. " + y.Firstname + " " + y.Middlename + " " + y.Lastname + " - N/A" : "Dr. " + y.Firstname + " " + y.Middlename + " " + y.Lastname + " - " + y.Contact
                                        });

            return getUser.ToList();
        }

        public List<SelectUser> FilterUsersWalkin(int? departmentId)
        {
            var getUser = _context.User.Where(x => x.FacilityId.Equals(UserFacility) && x.DepartmentId.Equals(departmentId) && x.Level.Equals(_roles.Value.DOCTOR))
                                        .Select(y => new SelectUser
                                        {
                                            MdId = y.Id,
                                            DoctorName = string.IsNullOrEmpty(y.Contact) ? "Dr. " + y.Firstname + " " + y.Middlename + " " + y.Lastname + " - N/A" : "Dr. " + y.Firstname + " " + y.Middlename + " " + y.Lastname + " - " + y.Contact
                                        });

            return getUser.ToList();
        }

        public DashboardViewModel DashboardValues(string level)
        {
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();
            var activities = _context.Activity.Where(x => x.DateReferred.Year.Equals(DateTime.Now.Year) && x.ReferredTo.Equals(UserFacility));
            for (int x = 1; x <= 12; x++)
            {
                accepted.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(_status.Value.ACCEPTED) || i.Status.Equals(_status.Value.ARRIVED) || i.Status.Equals(_status.Value.ADMITTED))).Count());
                redirected.Add(activities.Where(i => i.DateReferred.Month.Equals(x) && (i.Status.Equals(_status.Value.REJECTED) || i.Status.Equals(_status.Value.TRANSFERRED))).Count());
            }
            var adminDashboard = new DashboardViewModel(accepted.ToArray(), redirected.ToArray());
            return adminDashboard;
        }

        #region HELPERS

        public int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        public int UserFacility => int.Parse(User.FindFirstValue("Facility"));
        public int UserDepartment => int.Parse(User.FindFirstValue("Department"));
        public int UserProvince => int.Parse(User.FindFirstValue("Province"));
        public int UserMuncity => int.Parse(User.FindFirstValue("Muncity"));
        public int UserBarangay => int.Parse(User.FindFirstValue("Barangay"));
        public string UserName => "Dr. " + User.FindFirstValue(ClaimTypes.GivenName) + " " + User.FindFirstValue(ClaimTypes.Surname);

        #endregion
    }
}