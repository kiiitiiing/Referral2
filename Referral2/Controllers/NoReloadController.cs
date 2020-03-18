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
using MoreLinq.Extensions;
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
            var incoming = _context.Tracking
                .Where(x => x.ReferredTo == UserFacility && x.UpdatedAt >= DateTime.Now.Date);

            return incoming.Count();
        }

        public List<SelectDepartment> AvailableDepartments(int facilityId)
        {
            var availableDepartments = _context.User
                .Where(x => x.FacilityId.Equals(facilityId) && x.Level.Equals(_roles.Value.DOCTOR) && x.DepartmentId != null)
                .DistinctBy(d => d.DepartmentId)
                .Select(x => new SelectDepartment
                {
                    DepartmentId = (int)x.DepartmentId,
                    DepartmentName = x.Department.Description
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
                Status = status == "onDuty" ? "login" : "login_off",
                Logout = default,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            currentUser.LoginStatus = status == "onDuty" ? "login" : "login_off";
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
            string facilityAddress = facility.Address == null ? "" : facility.Address + ", ";
            string barangay = facility.Barangay == null ? "" : facility.Barangay.Description + ", ";
            string muncity = facility.Muncity == null ? "" : facility.Muncity.Description + ", ";
            string province = facility.Province == null ? "" : facility.Province.Description;
            string address = facilityAddress + barangay + muncity + province;

            var departments = _context.Department.Select(x => new SelectDepartment
            {
                DepartmentId = x.Id,
                DepartmentName = x.Description
            });

            var faciliyDepartment = _context.User
                .Include(x => x.Department)
                .Where(x => x.FacilityId.Equals(UserFacility) && x.Level.Equals(_roles.Value.DOCTOR) && x.DepartmentId != null)
                .Select(y => y.Department)
                .Distinct()
                .Select(x => new SelectDepartment
                {
                    DepartmentId = (int)x.Id,
                    DepartmentName = x.Description
                });

            SelectAddressDepartment selectAddress = new SelectAddressDepartment(address, faciliyDepartment);

            return selectAddress;
        }

        public async Task<int> SupportReport()
        {
            var incomingCtr = await _context.Tracking
                .Where(x=>x.Status == _status.Value.SEEN || x.Status == _status.Value.REFERRED)
                .Where(x => x.ReferredTo == UserFacility && x.DateReferred >= DateTime.Now.AddDays(-3)).CountAsync();

            return incomingCtr;
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
                    DoctorName = ("Dr. " + y.Firstname + " " + y.Middlename.CheckName() + " " + y.Lastname).NameToUpper() + " - " + y.Contact.CheckName()
                }); ;

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

        public async Task<int> RecoCount(string code)
        {
            var recoCtr = await _context.Feedback.Where(x => x.Code == code).CountAsync();

            return recoCtr;
        }

        public DashboardViewModel DashboardValues(string level)
        {
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();

            IQueryable<Activity> activities = null;

            if(!level.Equals(_roles.Value.ADMIN))
                activities = _context.Activity.Where(x => x.DateReferred.Year.Equals(DateTime.Now.Year) && x.ReferredTo.Equals(UserFacility));
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