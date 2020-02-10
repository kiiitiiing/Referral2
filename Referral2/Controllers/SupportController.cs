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
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Referral2.Models.ViewModels;

namespace Referral2.Controllers
{
    [Authorize(Policy = "support")]
    public class SupportController : Controller
    {
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

        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }
        // GET: ADD USER
        [HttpGet]
        public IActionResult AddUser()
        {
            var departments = _context.Department;
            ViewBag.Departments = new SelectList(departments, "Id", "Description");

            return PartialView();
        }
        // POST: ADD USER
        [HttpPost]
        public async Task<IActionResult> AddUser([Bind] AddDoctorViewModel model)
        {
            var departments = _context.Department;
            if (ModelState.IsValid)
            {
                if (await _userService.RegisterDoctorAsync(model, UserFacility))
                {
                    return PartialView(model);
                }
                else
                {
                    ModelState.AddModelError("Username", "Username already exists!");
                }
            }

            ViewBag.Departments = new SelectList(departments, "Id", "Description", model.Department);

            return PartialView(model);
        }
        // GET: CHAT
        public async Task<IActionResult> Chat()
        {
            var chats = await _context.Feedback
                .Where(x => x.Code.Equals("it-support-chat") && x.Sender.FacilityId.Equals(UserFacility))
                .Select(x=> new SupportChatViewModel 
                {
                    SupportId = (int)x.SenderId,
                    SupportFacilityName = x.Sender.Facility.Name,
                    SupportName = GlobalFunctions.GetFullName(x.Sender),
                    Message = x.Message,
                    MessageDate = (DateTime)x.CreatedAt
                }).AsNoTracking().ToListAsync();

            return View(chats);
        }
        // POST: CHAT
        [HttpPost]
        public async Task<IActionResult> Chat([Bind] FeedbackViewModel model)
        {
            if(ModelState.IsValid)
            {
                var chat = new Feedback
                {
                    Code = model.Code,
                    SenderId = model.MdId,
                    RecieverId = null,
                    Message = model.Message,
                    UpdatedAt = DateTime.Now,
                    CreatedAt = DateTime.Now
                };
                await _context.AddAsync(chat);
                await _context.SaveChangesAsync();
            };

            return View();
        }
        // GET: DAILY REFERRALS
        public async Task<IActionResult> DailyReferrals(string daterange, int? page)
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            if(!string.IsNullOrEmpty(daterange))
            {
                StartDate = DateTime.Parse(daterange.Substring(0, daterange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(daterange.Substring(daterange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            int size = 20;


            var tract = _context.Tracking
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Join(
                    _context.Activity,
                    t => t.Code,
                    a => a.Code,
                    (t, a) =>
                    new
                    {
                        ReferredTo = t.ReferredTo,
                        ReferredFrom = t.ReferredFrom,
                        ActionMd = a.ActionMd,
                        Status = a.Status,
                        ReferringMd = a.ReferringMd,
                        DateAction = a.DateReferred
                    }
                );
            var activities = _context.Activity.Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var trackings = _context.Tracking.Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var seens = _context.Seen.Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);

            var users = _context.User
                .Where(x => x.FacilityId.Equals(UserFacility) && x.Level.Equals(_roles.Value.DOCTOR))
                .OrderBy(x => x.Lastname)
                .Select(i => new DailyReferralSupport
                {
                    DoctorName = i.Firstname + " " + i.Middlename + " " + i.Lastname,
                    OutAccepted = activities.Where(x => x.ReferringMd.Equals(i.Id) && x.Status.Equals(_status.Value.ACCEPTED)).Count(),
                    OutRedirected = tract.Where(x => x.ReferredFrom.Equals(UserFacility) && x.ReferringMd.Equals(i.Id) && x.Status.Equals(_status.Value.REJECTED)).Count(),
                    OutSeen = trackings.Where(x => x.ReferringMd.Equals(i.Id) && x.DateSeen == default).Count(),
                    OutTotal = trackings.Where(x => x.ReferringMd.Equals(i.Id)).Count(),
                    InAccepted = tract.Where(x => x.ReferredTo.Equals(UserFacility) && x.ActionMd.Equals(i.Id) && x.Status.Equals(_status.Value.ACCEPTED)).Count(),
                    InRedirected = tract.Where(x => x.ReferredTo.Equals(UserFacility) && x.ActionMd.Equals(i.Id) && x.Status.Equals(_status.Value.REJECTED)).Count(),
                    InSeen = seens.Where(x=>x.UserMd.Equals(i.Id)).GroupBy(x=>x.TrackingId).Select(x=>x).Count(),
                });
            return View(await PaginatedList<DailyReferralSupport>.CreateAsync(users.AsNoTracking(), page ?? 1, size));
        }
        // GET: DAILY USERS
        public async Task<IActionResult> DailyUsers(string date, int? page)
        {
            StartDate = DateTime.Now.Date;
            if (date != null)
            {
                page = 1;
                StartDate = DateTime.Parse(date);
            }
            int size = 20;

            var month = StartDate.Month;
            var day = StartDate.Day;

            ViewBag.Date = StartDate;

            var dailyUsers = _context.User
                .Where(x => x.FacilityId.Equals(UserFacility) && x.Level.Equals(_roles.Value.DOCTOR) && ((DateTime)x.UpdatedAt).Date == StartDate)
                .Select(x => new DailyUsersViewModel
                {
                    MDName = GlobalFunctions.GetFullLastName(x),
                    OnDuty = x.LoginStatus,
                    LoggedIn = x.LoginStatus.Contains("login") ? true : false,
                    LoginTime = x.LastLogin,
                    LogoutTime = (DateTime)x.UpdatedAt
                });


            return View(await PaginatedList<DailyUsersViewModel>.CreateAsync(dailyUsers.AsNoTracking(), page ?? 1, size));
        }
        // GET: HOSPITAL INFO
        [HttpGet]
        public IActionResult HospitalInfo()
        {
            var facility = _context.Facility.Find(UserFacility);

            var currentFacility = new HospitalInfoViewModel
            {
                Id = facility.Id,
                FacilityName = facility.Name,
                Abbreviation = facility.Abbrevation,
                ProvinceId = facility.ProvinceId,
                MuncityId = (int)facility.MuncityId,
                BarangayId = (int)facility.BarangayId,
                Address = facility.Address,
                Contact = facility.Contact,
                Email = facility.Email,
                Status = (int)facility.Status
            };
            var muncities = _context.Muncity.Where(x => x.ProvinceId.Equals(facility.ProvinceId));
            var barangays = _context.Barangay.Where(x => x.ProvinceId.Equals(facility.ProvinceId) && x.MuncityId.Equals(facility.MuncityId));

            ViewBag.Muncities = new SelectList(muncities, "Id", "Description", currentFacility.MuncityId);
            ViewBag.Barangays = new SelectList(barangays, "Id", "Description", currentFacility.BarangayId);
            ViewBag.Statuses = new SelectList(ListContainer.Status, "Key", "Value");

            return View(currentFacility);
        }
        // POST: HOSPITAL INFO
        [HttpPost]
        public async Task<IActionResult> HospitalInfo([Bind] HospitalInfoViewModel model)
        {
            var facilities = _context.Facility.Where(x => x.Id != UserFacility);
            if (ModelState.IsValid)
            {
                if (!facilities.Any(x => x.Name == model.FacilityName))
                {
                    var updateFacility = UpdateFacility(model);
                    _context.Update(updateFacility);
                    await _context.SaveChangesAsync();
                }
                else
                    ModelState.AddModelError("FacilityName", "Facility name already Exists!");
            }
            var muncities = _context.Muncity.Where(x => x.ProvinceId.Equals(model.ProvinceId));
            var barangays = _context.Barangay.Where(x => x.ProvinceId.Equals(model.ProvinceId) && x.MuncityId.Equals(model.MuncityId));

            ViewBag.Muncities = new SelectList(muncities, "Id", "Description", model.MuncityId);
            ViewBag.Barangays = new SelectList(barangays, "Id", "Description", model.BarangayId);
            ViewBag.Statuses = new SelectList(ListContainer.Status, "Key", "Value", model.Status);
            return View(model);
        }
        // GET: INCOMING REPORT 
        public async Task<IActionResult> IncomingReport()
        {
            var incoming = await _context.Tracking
                .Where(x => x.ReferredTo.Equals(UserFacility))
                .Select(x => new IncomingReferralViewModel
                {
                    PatientName = GlobalFunctions.GetFullName(x.Patient),
                    ReferringFacility = x.ReferredFromNavigation.Name,
                    Department = x.Department.Description,
                    DateReferred = x.DateReferred,
                    Status = x.Status
                }).AsNoTracking().ToListAsync();

            return View(incoming);
        }
        // GET: MANANGE USERS
        public async Task<IActionResult> ManageUsers(string searchName)
        {
            ViewBag.SearchString = searchName;
            var doctors = _context.User
                .Where(x => x.FacilityId.Equals(UserFacility) && x.Level.Equals(_roles.Value.DOCTOR))
                .Select(y => new SupportManageViewModel
                {
                    Id = y.Id,
                    DoctorName = y.Firstname + " " + y.Middlename + " " + y.Lastname,
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
        // GET: DASHBOARD
        public IActionResult SupportDashboard()
        {
            List<int> accepted = new List<int>();
            List<int> redirected = new List<int>();

            var activities = _context.Activity;
            var totalDoctors = _context.User
                .Where(x => x.Level.Equals(_roles.Value.DOCTOR) && x.FacilityId.Equals(UserFacility)).Count();
            var onlineDoctors = _context.User
                .Where(x => x.Login.Equals("login") && x.FacilityId.Equals(UserFacility)).Count();
            var referredPatients = _context.Tracking
                .Where(x => x.ReferredTo.Equals(UserFacility)).Count();
            var dashboard = new SupportDashboadViewModel(
                totalDoctors,
                onlineDoctors,
                referredPatients);

            return View(dashboard);
        }
        // GET: UPDATE USER
        [HttpGet]
        public async Task<IActionResult> UpdateUser(int? id)
        {
            var departments = _context.Department;
            var currentMd = await _context.User.FindAsync(id);
            if(currentMd != null)
            {
                ViewBag.Status = new SelectList(ListContainer.UserStatus, "Key", "Value", currentMd.Status);
                ViewBag.Departments = new SelectList(departments, "Id", "Description", currentMd.DepartmentId);
                currentMd.Password = "";
            }

            var doctor = returnDoctorInfo(currentMd);

            return PartialView(doctor);
        }
        // POST: UPDATE USER
        [HttpPost]
        public async Task<IActionResult> UpdateUser([Bind] UpdateDoctorViewModel model)
        {
            var departments = _context.Department;
            var doctor = await SetDoctorViewModel(model);
            if (ModelState.IsValid)
            {
                var doctors = _context.User.Where(x => x.Id != doctor.Id);
                if(!doctors.Any(x => x.Username.Equals(model.Username)))
                {
                    if (!string.IsNullOrEmpty(model.Password))
                        _userService.ChangePasswordAsync(doctor, model.Password);
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    return PartialView("~/Views/Support/UpdateUser.cshtml", model);
                }
                else
                {
                    ModelState.AddModelError("Username", "Username already exists!");
                }
            }
            ViewBag.Status = new SelectList(ListContainer.Status, "Key", "Value", doctor.Status);
            ViewBag.Departments = new SelectList(departments, "Id", "Description", doctor.DepartmentId);
            return PartialView("~/Views/Support/UpdateUser.cshtml",model);
        }
        #region HELPERS

        private async Task<User> SetDoctorViewModel(UpdateDoctorViewModel model)
        {
            var doctor = await _context.User.FindAsync(model.Id);

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
                Id = doctor.Id,
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
            };

            return returnDoctor;
        }
        public Facility UpdateFacility(HospitalInfoViewModel model)
        {
            var facility = _context.Facility.Find(UserFacility);
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
        public int UserFacility => int.Parse(User.FindFirstValue("Facility"));

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