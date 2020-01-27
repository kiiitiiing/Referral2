using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Referral2.Models.ViewModels.Admin;
using Referral2.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class AdminController : Controller
    {
        const string SessionSupportUsername = "_username";
        private readonly IUserService _userService;
        public readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public AdminController(ReferralDbContext context, IUserService userService, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _userService = userService;
            _roles = roles;
            _status = status;
        }

        public DateTime Date { get; set; }

        public IActionResult AdminDashboard()
        {
            var activities = _context.Activity.Where(x=>x.DateReferred.Year.Equals(DateTime.Now.Year));
            var totalDoctors = _context.User.Where(x => x.Level.Equals("doctor")).Count();
            var onlineDoctors = _context.User.Where(x => x.LoginStatus.Contains("login") ).Count();
            var activeFacility = _context.Facility.Count();
            var referredPatients = _context.Tracking.Where(x => x.DateReferred != default || x.DateAccepted != default || x.DateArrived != default).Count();

            var adminDashboard = new AdminDashboardViewModel(totalDoctors, onlineDoctors, activeFacility, referredPatients);

            return View(adminDashboard);
        }

        public async Task<IActionResult> SupportUsers()
        {
            var support = _context.User.Where(x => x.Level.Equals(_roles.Value.SUPPORT))
                            .Select(x => new SupportUsersViewModel
                            {
                                Id = x.Id,
                                Name = x.Firstname + " " + x.Middlename + " " + x.Lastname,
                                Facility = x.Facility.Name,
                                Contact = x.Contact ?? "N/A",
                                Email = x.Email ?? "N/A",
                                Username = x.Username,
                                Status = x.LoginStatus,
                                LastLogin = x.LastLogin == null ? default : x.LastLogin
                            });

            return View(await support.ToListAsync());
        }

        public async Task<IActionResult> Facilities(int? pageNumber, string search)
        {
            ViewBag.Search = search;
            var facilities = _context.Facility
                            .Select(x => new FacilitiesViewModel
                            {
                                Id = x.Id,
                                Facility = x.Name,
                                Address = x.Address,
                                Contact = x.Contact,
                                Email = x.Email,
                                Chief = x.ChiefHospital ?? "N/A",
                                Level = x.HospitalLevel,
                                Type = x.HospitalType ?? "N/A"
                            });

            int pageSize = 10;

            if(!string.IsNullOrEmpty(search))
            {
                facilities = facilities.Where(x => x.Facility.Contains(search));
            }

            return View(await facilities.OrderBy(a => a.Facility).ToListAsync());
            //return View(await PaginatedList<FacilitiesViewModel>.CreateAsync(facilities.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public IActionResult AddFacility()
        {
            var provinces = _context.Province;
            ViewBag.Provinces = new SelectList(provinces, "Id", "Description");
            ViewBag.HospitalLevels = new SelectList(ListContainer.HospitalLevel);
            ViewBag.HospitalTypes = new SelectList(ListContainer.HospitalType);
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AddFacility([Bind] FacilityViewModel model)
        {
            if(ModelState.IsValid)
            {
                var faciliy = await SetFacilityViewModelAsync(model);
                await _context.AddAsync(faciliy);
                await _context.SaveChangesAsync();
            }
            else
            {
                if (model.Province == null)
                    ModelState.AddModelError("Province", "Please select a province.");
                if (model.Muncity == null)
                    ModelState.AddModelError("Muncity", "Please select a municipality/city.");
                if (model.Barangay == null)
                    ModelState.AddModelError("Barangay", "Please select a barangay.");
                if (model.Level == null)
                    ModelState.AddModelError("Level", "Please select hospital level.");
                if (model.Type == null)
                    ModelState.AddModelError("Type", "Plase select hospital type.");
            }
            var provinces = _context.Province;
            ViewBag.Provinces = new SelectList(provinces, "Id", "Description",model.Province);
            return PartialView(model);
        }

        

        public async Task<IActionResult> UpdateFacility(int? id)
        {
            var facilityModel =await _context.Facility.FindAsync(id);

            var facility = await SetFacilityModel(facilityModel);

            var province = _context.Province;
            var muncity = _context.Muncity.Where(x => x.ProvinceId.Equals(facility.Province));
            var barangay = _context.Barangay.Where(x => x.MuncityId.Equals(facility.Barangay));

            ViewBag.Provinces = new SelectList(province, "Id", "Description", facility.Province);
            ViewBag.Muncities = new SelectList(muncity, "Id", "Description", facility.Muncity);
            ViewBag.Barangays = new SelectList(barangay, "Id", "Description", facility.Barangay);

            return PartialView(facility);
        }

        

        [HttpPost]
        public async Task<IActionResult> UpdateFacility([Bind] Facility model)
        {
            if(ModelState.IsValid)
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
            }

            return PartialView(model);
        }

        public async Task<IActionResult> OnlineUsers(string date)
        {
            Date = DateTime.Now.Date;

            if(!string.IsNullOrEmpty(date))
            {
                Date = DateTime.Parse(date);
            }

            ViewBag.Date = Date.ToString("dd/MM/yyyy");

            var onlineUsers = await _context.User
                .Select(x => new OnlineAdminViewModel
                {
                    FacilityName = x.Facility.Name,
                    UserFullName = GlobalFunctions.GetFullLastName(x),
                    UserLevel = x.Level,
                    UserDepartment = x.Department.Description,
                    UserStatus = x.LoginStatus,
                    UserLoginTime = x.LastLogin
                })
                .Where(x => x.UserLoginTime.Date.Equals(Date))
                .OrderBy(x => x.FacilityName)
                .AsNoTracking()
                .ToListAsync();

            return View(onlineUsers);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateSupport(int? id)
        {
            var facility = _context.Facility;
            var currentSupport = await _context.User.FindAsync(id);
            if (currentSupport != null)
            {
                ViewBag.Status = new SelectList(ListContainer.Status, currentSupport.Status);
                ViewBag.Facility = new SelectList(facility, "Id", "Name", currentSupport.FacilityId);
                currentSupport.Password = "";
            }

            var support = ReturnSupportInfo(currentSupport);
            HttpContext.Session.SetString(SessionSupportUsername, currentSupport.Username);
            return PartialView(support);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSupport([Bind] UpdateSupportViewModel model)
        {
            var doctorLastUsername = HttpContext.Session.GetString(SessionSupportUsername);
            var facilities = _context.Facility;
            var support = SetSupportViewModel(model);
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(model.Password))
                {
                    if (_userService.ChangePasswordAsync(support, model.Password))
                    {
                        return RedirectToAction("ManageUsers", "Support");
                    }
                }
                else
                {
                    if (!model.Username.Equals(doctorLastUsername))
                    {
                        if (!_context.User.Any(x => x.Username.Equals(model.Username)))
                        {
                            _context.Update(support);
                            await _context.SaveChangesAsync();
                            return RedirectToAction("ManageUsers", "Support");
                        }
                        else
                        {
                            ModelState.AddModelError("Username", "Username already exist");
                        }

                    }
                }

            }
            ViewBag.Statuses = new SelectList(ListContainer.Status, support.Status);
            ViewBag.Facilities = new SelectList(facilities, "Id", "Description", support.FacilityId);
            return PartialView("~/Views/Admin/UpdateSupport.cshtml", model);
        }

        

        public IActionResult AddSupport()
        {
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x=>x.Name.Contains("")), "Id", "Name");
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> AddSupport([Bind] AddSupportViewModel model)
        {
            if(ModelState.IsValid)
            {
                model.Firstname = GlobalFunctions.FixName(model.Firstname);
                model.Middlename = GlobalFunctions.FixName(model.Middlename);
                model.Lastname = GlobalFunctions.FixName(model.Lastname);
                if (await _userService.RegisterDoctorAsync(model))
                {
                    return RedirectToAction("SupportUsers");
                }

            }
            ViewBag.Facilities = new SelectList(_context.Facility, "Id", "Name");
            return PartialView(model);
        }



        #region HELPERS
        private Task<FacilityViewModel> SetFacilityModel(Facility facility)
        {
            var facilityModel = new FacilityViewModel
            {
                Id = facility.Id,
                Name = facility.Name,
                Abbrevation = facility.Abbrevation,
                Province = facility.ProvinceId,
                Muncity = facility.MuncityId,
                Barangay = facility.BarangayId,
                Address = facility.Address,
                Contact = facility.Contact,
                Email = facility.Email,
                Chief = facility.ChiefHospital,
                Level = facility.HospitalLevel,
                Type = facility.HospitalType
            };

            return Task.FromResult(facilityModel);
        }
        private Task<Facility> SetFacilityViewModelAsync(FacilityViewModel model)
        {
            var facility = new Facility
            {
                Name = model.Name,
                Abbrevation = model.Abbrevation,
                Address = model.Address,
                BarangayId = model.Barangay,
                MuncityId = model.Muncity,
                ProvinceId = (int)model.Province,
                Contact = model.Contact,
                Email = model.Email,
                Status = 1,
                Picture = "",
                ChiefHospital = model.Chief,
                HospitalLevel = (int)model.Level,
                HospitalType = model.Type
            };

            return Task.FromResult(facility);
        }
        private UpdateSupportViewModel ReturnSupportInfo(User currentSupport)
        {
            var support = new UpdateSupportViewModel
            {
                Firstname = currentSupport.Firstname,
                Middlename = currentSupport.Middlename,
                Lastname = currentSupport.Lastname,
                ContactNumber = currentSupport.Contact,
                Email = currentSupport.Email,
                Facility = currentSupport.FacilityId,
                Designation = currentSupport.Designation,
                Status = currentSupport.Status,
                Username = currentSupport.Username
            };
            return support;
        }
        private User SetSupportViewModel(UpdateSupportViewModel model)
        {
            var support = _context.User.First(x => x.Username.Equals(model.Username));

            support.Firstname = model.Firstname;
            support.Middlename = model.Middlename;
            support.Lastname = model.Lastname;
            support.Contact = model.ContactNumber;
            support.Email = model.Email;
            support.Designation = model.Designation;
            support.FacilityId = model.Facility;
            support.Status = model.Status;
            support.Username = model.Username;
            support.UpdatedAt = DateTime.Now;
            return support;
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