using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Globalization;
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
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication.Cookies;
using Referral2.Models.ViewModels.Consolidated;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Administrator")]
    public class AdminController : Controller
    {
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


        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public DateTime Date { get; set; }

        // DASHBOARD
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

        // GET: ADD FACILITY
        public IActionResult AddFacility()
        {
            var provinces = _context.Province;
            ViewBag.Provinces = new SelectList(provinces, "Id", "Description");
            ViewBag.HospitalLevels = new SelectList(ListContainer.HospitalLevel, "Key", "Value");
            ViewBag.HospitalTypes = new SelectList(ListContainer.HospitalType, "Key", "Value");
            return PartialView();
        }

        // POST: ADD FACILITY
        [HttpPost]
        public async Task<IActionResult> AddFacility([Bind] FacilityViewModel model)
        {
            if (ModelState.IsValid)
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
            ViewBag.Provinces = new SelectList(provinces, "Id", "Description", model.Province);
            if(model.Muncity != null)
            {
                var muncities = _context.Muncity.Where(x => x.ProvinceId == model.Province);
                ViewBag.Muncities = new SelectList(muncities, "Id", "Description", model.Muncity);
            }
            if(model.Barangay != null)
            {
                var barangays = _context.Barangay.Where(x => x.ProvinceId == model.Province && x.MuncityId == model.Muncity);
                ViewBag.Barangays = new SelectList(barangays, "Id", "Description", model.Barangay);
            }
            ViewBag.HospitalLevels = new SelectList(ListContainer.HospitalLevel, "Key", "Value", model.Level);
            ViewBag.HospitalTypes = new SelectList(ListContainer.HospitalType, "Key", "Value", model.Type);
            return PartialView(model);
        }

        // GET: ADD SUPPORT
        public IActionResult AddSupport()
        {
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.Name.Contains("")), "Id", "Name");
            return PartialView();
        }
        // POST: ADD SUPPORT
        [HttpPost]
        public async Task<IActionResult> AddSupport([Bind] AddSupportViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _userService.RegisterSupportAsync(model))
                {
                    return RedirectToAction("SupportUsers");
                }

            }
            ViewBag.Facilities = new SelectList(_context.Facility, "Id", "Name");
            return PartialView(model);
        }

        // CONSOLIDATED
        public async Task<IActionResult> Consolidated(string dateRange)
        {
            var dateNow = DateTime.Now;
            StartDate = new DateTime(dateNow.Year, dateNow.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            // TABLES

            var patientForms = _context.PatientForm
                .Where(x => x.TimeReferred >= StartDate && x.TimeReferred <= EndDate);

            var pregnantForms = _context.PregnantForm
                .Where(x => x.ReferredDate >= StartDate && x.ReferredDate <= EndDate);

            var facilities = _context.Facility;

            var trackings = _context.Tracking
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);

            var activities = _context.Activity
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);

            var trans = _context.Transportation;

            var departments = _context.Department;

            var doctors = _context.User;
            // --------------------- INCOMING ---------------------
            #region INCOMING
            // INCOMING: INCOMING
            var inIncoming = await trackings
                .Where(x => x.ReferredTo.Equals(UserFacility) && x.DateReferred >= StartDate && x.DateReferred <= EndDate).CountAsync();
            // INCOMING: ACCEPTED
            var inAccepted = await _context.Tracking
                .Where(x => x.ReferredTo == UserFacility && x.DateAccepted != default && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .CountAsync();
            // INCOMING: REFERRING FACILITY
            var inReferringFac = await trackings
                .Where(x => x.ReferredTo == UserFacility)
                .GroupBy(x => x.ReferredFrom)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = facilities.SingleOrDefault(x => x.Id == i.Key).Name
                })
                .ToListAsync();
            // INCOMING: REFERRING DOCTOS
            var inReferringDoc = await trackings
                .Where(x => x.ReferredTo == UserFacility)
                .GroupBy(x => x.ReferringMd)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = i.Key == null ? "" : GlobalFunctions.GetMDFullName(doctors.SingleOrDefault(x => x.Id == i.Key))
                })
                .Take(10)
                .ToListAsync();


            // INCOMING: DIAGNOSIS

            var inDiagnosis = patientForms
               .Where(p => p.ReferredTo == UserFacility)
               .Select(p => p.Diagnosis)
               .AsEnumerable()
               .Concat(
                  pregnantForms
                     .Where(pf => pf.ReferredTo == UserFacility)
                     .Select(pf => pf.WomanMajorFindings)
               )
               .GroupBy(u => u)
               .Select(
                  x =>
                     new ListItem
                     {
                         NoItem = x.Count(),
                         ItemName = x.Key
                     })
               .Take(10);

            // INCOMING: REASONS
            var inReason = patientForms
               .Where(p => p.ReferredTo == UserFacility)
               .Select(p => p.Reason)
               .AsEnumerable()
               .Concat(
                  pregnantForms
                     .Where(pf => pf.ReferredTo == UserFacility)
                     .Select(pf => pf.WomanReason)
               )
               .GroupBy(u => u)
               .Select(
                  x =>
                     new ListItem
                     {
                         NoItem = x.Count(),
                         ItemName = x.Key
                     })
               .Take(10);
            // INCOMING: TRANSPORTATIONS
            var inTransportation = await trackings
                .Where(x => x.ReferredTo == UserFacility)
                .GroupBy(x => x.Transportation)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = string.IsNullOrEmpty(i.Key) ? "" : i.Key
                })
               .ToListAsync();
            // INCOMING: DEPARTMENT
            var inDepartment = await trackings
                .Where(x => x.ReferredTo == UserFacility)
                .GroupBy(x => x.DepartmentId)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = departments.SingleOrDefault(x => x.Id.Equals(i.Key)).Description
                })
               .ToListAsync();
            #endregion

            // --------------------- OUTGOING ---------------------
            #region OUTGOING
            // OUTGOING: OUTGOING
            var outOutgoing = await trackings
                .Where(x => x.ReferredTo.Equals(UserFacility) && x.DateReferred >= StartDate && x.DateReferred <= EndDate).CountAsync();
            // OUTGOING: ACCEPTED
            var outAccepted = await trackings
                .Where(x => x.ReferredTo == UserFacility && x.DateAccepted != default)
                .CountAsync();
            // OUTGOING: REDIRECTED
            var outRedirected = await activities
                .Where(x => x.ReferredFrom.Equals(UserFacility) && x.Status.Equals(_status.Value.REJECTED))
                .CountAsync();
            // OUTGOING: ARCHIVED
            var outArchived = await trackings
                .Where(x => x.ReferredFrom.Equals(UserFacility))
                .Where(x => x.Status.Equals(_status.Value.REFERRED) || x.Status.Equals(_status.Value.SEEN))
                //.Where(x => GlobalFunctions.ArchivedTime(x.DateReferred) > 4320)
                .CountAsync();
            // OUTGOING: REFERRING FACILITY
            var outReferringFac = await trackings
                .Where(x => x.ReferredFrom == UserFacility)
                .GroupBy(x => x.ReferredTo)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = facilities.SingleOrDefault(x => x.Id == i.Key).Name
                })
                .ToListAsync();
            // OUTGOING: REFERRING DOCTOS
            var outReferringDoc = await trackings
                .Where(x => x.ReferredFrom == UserFacility)
                .GroupBy(x => x.ReferringMd)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = i.Key == null ? "" : GlobalFunctions.GetMDFullName(doctors.SingleOrDefault(x => x.Id == i.Key))
                })
                .Take(10)
                .ToListAsync();


            // OUTGOING: DIAGNOSIS

            var outDiagnosis = patientForms
               .Where(p => p.ReferringFacilityId == UserFacility)
               .Select(p => p.Diagnosis)
               .AsEnumerable()
               .Concat(
                  pregnantForms
                     .Where(pf => pf.ReferringFacility == UserFacility)
                     .Select(pf => pf.WomanMajorFindings)
               )
               .GroupBy(u => u)
               .Select(
                  x =>
                     new ListItem
                     {
                         NoItem = x.Count(),
                         ItemName = x.Key
                     })
               .Take(10);

            // OUTGOING: REASONS
            var outReason = patientForms
               .Where(p => p.ReferringFacilityId == UserFacility)
               .Select(p => p.Reason)
               .AsEnumerable()
               .Concat(
                  pregnantForms
                     .Where(pf => pf.ReferringFacility == UserFacility)
                     .Select(pf => pf.WomanReason)
               )
               .GroupBy(u => u)
               .Select(
                  x =>
                     new ListItem
                     {
                         NoItem = x.Count(),
                         ItemName = x.Key
                     })
               .Take(10);
            // OUTGOING: TRANSPORTATIONS
            var outTransportation = await trackings
                .Where(x => x.ReferredFrom == UserFacility)
                .GroupBy(x => x.Transportation)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = string.IsNullOrEmpty(i.Key) ? "" : i.Key
                })
               .ToListAsync();
            // OUTGOING: DEPARTMENT
            var outDepartment = await trackings
                .Where(x => x.ReferredFrom == UserFacility)
                .GroupBy(x => x.DepartmentId)
                .Select(i => new ListItem
                {
                    NoItem = i.Count(),
                    ItemName = departments.SingleOrDefault(x => x.Id.Equals(i.Key)).Description
                })
               .ToListAsync();

            #endregion

            var consolidated = new ConsolidatedViewModel
            {
                InIncoming = inIncoming,
                InAccepted = inAccepted,
                InViewed = inIncoming - inAccepted,
                InReferringFacilities = inReferringFac,
                InReferringDoctors = inReferringDoc,
                InDiagnosis = inDiagnosis.ToList(),
                InReason = inReason.ToList(),
                InTransportation = inTransportation,
                InDepartment = inDepartment,
                InRemarks = null,
                InAcceptance = 0,
                InArrival = 0,
                InHorizontal = 0,
                InVertical = inIncoming,
                OutOutgoing = outOutgoing,
                OutAccepted = outAccepted,
                OutViewed = outOutgoing - outAccepted,
                OutRedirected = outRedirected,
                OutArchived = outArchived,
                OutViewedAcceptance = 0,
                OutViewedRedirection = 0,
                OutAcceptance = 0,
                OutRedirection = 0,
                OutTransport = 0,
                OutHorizontal = 0,
                OutVertical = 0,
                OutReferredFacility = outReferringFac,
                OutReferredDoctor = outReferringDoc,
                OutDiagnosis = outDiagnosis.ToList(),
                OutReason = outReason.ToList(),
                OutTransportation = outTransportation,
                OutDepartment = outDepartment,
                OutRemarks = null

            };

            return View(consolidated);
        }

        // DAILY REFERRAL
        public async Task<IActionResult> DailyReferral(string dateRange)
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;


            var tracking = _context.Tracking.Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var activity = _context.Activity.Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var rejected = tracking
               .Join(
                  activity,
                  t => t.Code,
                  a => a.Code,
                  (t, a) =>
                     new
                     {
                         ReferredTo = t.ReferredTo,
                         Status = a.Status,
                         DateReferred = a.DateReferred
                     });

            var seen = _context.Seen.Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);


            var facilities = await _context.Facility
                .Select(i => new DailyReferralViewModel
                {
                    Facility = i.Name,
                    AcceptedTo = activity.Where(x => x.ReferredTo.Equals(i.Id) && x.DateReferred >= StartDate && x.DateReferred <= EndDate && x.Status.Equals(_status.Value.ACCEPTED)).Count(),
                    RedirectedTo = rejected.Where(x => x.ReferredTo.Equals(i.Id) && x.Status.Equals(_status.Value.REJECTED) && x.DateReferred >= StartDate && x.DateReferred <= EndDate).Count(),
                    SeenTo = seen.Where(x => x.Tracking.ReferredTo.Equals(i.Id) && x.Tracking.DateSeen >= StartDate && x.Tracking.DateSeen <= EndDate).Count(),
                    AcceptedFrom = activity.Where(x => x.ReferredFrom.Equals(i.Id) && x.Status.Equals(_status.Value.ACCEPTED) && x.DateReferred >= StartDate && x.DateReferred >= EndDate).Count(),
                    RedirectedFrom = activity.Where(x => x.ReferredFrom.Equals(i.Id) && x.Status.Equals(_status.Value.REJECTED) && x.DateReferred >= StartDate && x.DateReferred >= EndDate).Count(),
                    SeenFrom = seen.Where(x => x.Tracking.ReferredFrom.Equals(i.Id) && x.Tracking.DateSeen >= StartDate && x.Tracking.DateSeen <= EndDate).Count()
                })
                .OrderBy(x => x.Facility)
                .AsNoTracking()
                .ToListAsync();

            return View(facilities);
        }

        // DAILY USERS
        public async Task<IActionResult> DailyUsers(string date)
        {
            var users = _context.Login;

            Date = DateTime.Now.Date;

            if (!string.IsNullOrEmpty(date))
            {
                Date = DateTime.Parse(date).Date;
            }

            ViewBag.Date = Date.ToString("dd/MM/yyyy");

            var facilities = await _context.Facility
                .Select(i => new DailyUsersAdminModel
                {
                    Facility = i.Name,
                    OnDutyHP = users.Where(x => x.User.Level.Equals(_roles.Value.DOCTOR) && x.Status.Equals("login") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OffDutyHP = users.Where(x => x.User.Level.Equals(_roles.Value.DOCTOR) && x.Status.Equals("login off") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OfflineHP = users.Where(x => x.User.Level.Equals(_roles.Value.DOCTOR) && x.Status.Equals("logout") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OnlineIT = users.Where(x => x.User.Level.Equals(_roles.Value.SUPPORT) && !x.Status.Equals("logout") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                    OfflineIT = users.Where(x => x.User.Level.Equals(_roles.Value.SUPPORT) && x.Status.Equals("logout") && x.User.FacilityId.Equals(i.Id) && x.Login1.Date.Equals(Date)).Count(),
                })
                .OrderBy(x => x.Facility)
                .ToListAsync();

            return View(facilities);
        }

        // FACILITIES
        public async Task<IActionResult> Facilities(int? page, string search, string filter)
        {
            if (search != null)
                page = 1;
            else
                search = filter;
            ViewBag.Filter = search;
            int size = 6;

            var facilities = _context.Facility
                            .Select(x => new FacilitiesViewModel
                            {
                                Id = x.Id,
                                Facility = x.Name,
                                Address = x.Address,
                                Contact = x.Contact,
                                Email = x.Email,
                                Chief = x.ChiefHospital,
                                Level = x.HospitalLevel,
                                Type = x.HospitalType
                            })
                            .OrderBy(x => x.Facility)
                            .AsQueryable();


            if(!string.IsNullOrEmpty(search))
            {
                facilities = facilities.Where(x => x.Facility.Contains(search));
            }

            return View(await PaginatedList<FacilitiesViewModel>.CreateAsync(facilities.AsNoTracking(), page ?? 1, size));
        }

        // GRAPH
        public async Task<IActionResult> Graph(string year)
        {
            var yearNow = DateTime.Now.Year;

            if (!string.IsNullOrEmpty(year))
            {
                yearNow = int.Parse(year);
            }
            StartDate = new DateTime(yearNow, 1, 1);
            EndDate = new DateTime(yearNow, 12, 31);
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            var trackings = _context.Tracking
                .Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var activities = _context.Activity
                .Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var facilities = await _context.Facility
                .Select(i => new GraphValuesModel
                {
                    Facility = i.Name,
                    Incoming = trackings
                                    .Where(t => t.ReferredTo == i.Id)
                                    .Join(
                                        activities,
                                        t => t.Code,
                                        a => a.Code,
                                        (t, a) =>
                                            new
                                            {
                                                t = t,
                                                a = a
                                            }
                                    )
                                    .Count(),
                    Accepted = trackings
                                    .Where(t => t.ReferredTo == i.Id)
                                    .Join(
                                        activities,
                                        t => t.Code,
                                        a => a.Code,
                                        (t, a) =>
                                            new
                                            {
                                                t = t,
                                                a = a
                                            }
                                    )
                                    .Where(x => x.a.Status == _status.Value.ACCEPTED)
                                    .Count(),
                    Outgoing = trackings
                                    .Where(t => t.ReferredFrom == i.Id)
                                    .Join(
                                        activities,
                                        t => t.Code,
                                        a => a.Code,
                                        (t, a) =>
                                            new
                                            {
                                                t = t,
                                                a = a
                                            }
                                    )
                                    .Count(),
                })
                .Where(x => x.Incoming != 0 && x.Outgoing != 0 && x.Accepted != 0)
                .AsNoTracking()
                .ToListAsync();

            return View(facilities);
        }

        // GET: LOGIN AS
        public ActionResult Login()
        {
            var facilities = _context.Facility.Where(x => x.Status.Equals(1));
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            return View();
        }

        // POST: LOGIN AS
        [HttpPost]
        public async Task<IActionResult> Login(int facility, string level)
        {
            await LoginAsAsync(facility, level);
            if (level.Equals(_roles.Value.ADMIN))
                return RedirectToAction("AdminDashboard", "Admin");
            else if (level.Equals(_roles.Value.DOCTOR))
            {
                return RedirectToAction("Index", "Home");
            }
            else if (level.Equals(_roles.Value.SUPPORT))
            {
                return RedirectToAction("SupportDashboard", "Support");
            }
            else //if (level.Equals(_roles.Value.MCC))
            {
                return RedirectToAction("MccDashboard", "MedicalCenterChief");
            }
        }

        // ONLINE USERS
        public async Task<IActionResult> OnlineUsers(string date)
        {
            Date = DateTime.Now.Date;

            if(!string.IsNullOrEmpty(date))
            {
                Date = DateTime.Parse(date);
            }

            ViewBag.Date = Date.ToString("dd/MM/yyyy");

            var onlineUsers = await _context.User
                .Where(x => x.Level.Equals(_roles.Value.DOCTOR) && x.LastLogin >= Date)
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

        // INCOMING PATIENTS
        public async Task<IActionResult> PatientIncoming(string dateRange)
        {
            StartDate = DateTime.Now;
            EndDate = DateTime.Now;
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            var tracking = await _context.Tracking
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate && x.ReferredTo == UserFacility)
                .AsNoTracking()
                .ToListAsync();

            return View(tracking);
        }

        // REFERRAL STATUS
        public async Task<IActionResult> ReferralStatus(string dateRange)
        {
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = StartDate.AddMonths(1).AddDays(-1);
            var culture = CultureInfo.InvariantCulture;

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;

            var referrals = await _context.Tracking
                .Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate)
                .Select(x => new ReferralStatusViewModel
                {
                    DateReferred = x.DateReferred.ToString("MMM d, h:mm tt", culture),
                    FacilityFrom = x.ReferredFromNavigation.Name,
                    FacilityTo = x.ReferredToNavigation.Name,
                    Department = x.Department.Description,
                    PatientName = GlobalFunctions.GetFullLastName(x.Patient),
                    Status = x.Status
                })
                .ToListAsync();

            return View(referrals);
        }

        // REMOVE FACILITY
        public void RemoveFacility(int? id)
        {
            var facility = _context.Facility.Find(id);
            _context.Facility.Remove(facility);
            _context.SaveChanges();
        }

        // SUPPORT USERS
        public async Task<IActionResult> SupportUsers(int? page, string search, string filter)
        {
            if (search != null)
                page = 1;
            else
                search = filter;
            ViewBag.Filter = search;
            int size = 10;

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

            if (!string.IsNullOrEmpty(search))
                support = support.Where(x => x.Name.Contains(search));

            return View(await PaginatedList<SupportUsersViewModel>.CreateAsync(support.AsNoTracking(), page ?? 1, size));
        }

        // GET: UPDATE FACILITY
        [HttpGet]
        public async Task<IActionResult> UpdateFacility(int? id)
        {
            var facilityModel = await _context.Facility.FindAsync(id);

            var facility = await SetFacilityModel(facilityModel);

            var province = _context.Province;
            var muncity = _context.Muncity.Where(x => x.ProvinceId.Equals(facility.Province));
            var barangay = _context.Barangay.Where(x => x.MuncityId.Equals(facility.Muncity));

            ViewBag.Provinces = new SelectList(province, "Id", "Description", facility.Province);
            ViewBag.Muncities = new SelectList(muncity, "Id", "Description", facility.Muncity);
            ViewBag.Barangays = new SelectList(barangay, "Id", "Description", facility.Barangay);
            ViewBag.Levels = new SelectList(ListContainer.HospitalLevel, "Key", "Value", facility.Level);
            ViewBag.Types = new SelectList(ListContainer.HospitalType, "Key", "Value", facility.Type);

            return PartialView("~/Views/Admin/UpdateFacility.cshtml", facility);
        }

        // POST: UPDATE FACILITY
        [HttpPost]
        public async Task<IActionResult> UpdateFacility([Bind] FacilityViewModel model)
        {
            if (string.IsNullOrEmpty(model.Chief))
                model.Chief = "";
            if (ModelState.IsValid)
            {
                var facilities = _context.Facility.Where(x => x.Id != model.Id);
                if (!facilities.Any(x => x.Name.Equals(model.Name)))
                {
                    var saveFacility = await SaveFacilityAsync(model);
                    _context.Update(saveFacility);
                    await _context.SaveChangesAsync();
                    return PartialView("~/Views/Admin/UpdateFacility.cshtml", model);
                }
                else
                {
                    ModelState.AddModelError("Name", "Facility name already exists!");
                }
            }

            var province = _context.Province;
            var muncity = _context.Muncity.Where(x => x.ProvinceId.Equals(model.Province));
            var barangay = _context.Barangay.Where(x => x.MuncityId.Equals(model.Barangay));

            ViewBag.Provinces = new SelectList(province, "Id", "Description", model.Province);
            ViewBag.Muncities = new SelectList(muncity, "Id", "Description", model.Muncity);
            ViewBag.Barangays = new SelectList(barangay, "Id", "Description", model.Barangay);
            ViewBag.Levels = new SelectList(ListContainer.HospitalLevel, "Key", "Value", model.Level);
            ViewBag.Types = new SelectList(ListContainer.HospitalType, "Key", "Value", model.Type);

            return PartialView("~/Views/Admin/UpdateFacility.cshtml", model);
        }

        private async Task<Facility> SaveFacilityAsync(FacilityViewModel model)
        {
            var facility = await _context.Facility.FindAsync(model.Id);
            facility.Name = model.Name;
            facility.Abbrevation = model.Abbrevation;
            facility.ProvinceId = (int)model.Province;
            facility.MuncityId = (int)model.Muncity;
            facility.BarangayId = (int)model.Barangay;
            facility.Address = model.Address;
            facility.Contact = model.Contact;
            facility.Email = model.Email;
            facility.ChiefHospital = model.Chief;
            facility.HospitalLevel = (int)model.Level;
            facility.HospitalType = model.Type;

            return facility;
        }

        // GET: UPDATE SUPPORT
        [HttpGet]
        public async Task<IActionResult> UpdateSupport(int? id)
        {
            var facility = _context.Facility;
            var currentSupport = await _context.User.FindAsync(id);
            if (currentSupport != null)
            {
                ViewBag.Status = new SelectList(ListContainer.UserStatus, "Key", "Value", currentSupport.Status);
                ViewBag.Facility = new SelectList(facility, "Id", "Name", currentSupport.FacilityId);
                currentSupport.Password = "";
            }

            var support = ReturnSupportInfo(currentSupport);
            return PartialView(support);
        }

        // POST: UPDATE SUPPORT
        [HttpPost]
        public async Task<IActionResult> UpdateSupport([Bind] UpdateSupportViewModel model)
        {
            var facilities = _context.Facility;
            var support = await SetSupportViewModel(model);
            if (ModelState.IsValid)
            {
                var supports = _context.User.Where(x => x.Id != support.Id);
                if (!supports.Any(x => x.Username.Equals(model.Username)))
                {
                    if (!string.IsNullOrEmpty(model.Password))
                        _userService.ChangePasswordAsync(support, model.Password);
                    _context.Update(support);
                    await _context.SaveChangesAsync();
                    return PartialView("~/Views/Admin/UpdateSupport.cshtml", model);
                }
                else
                {
                    ModelState.AddModelError("Username", "Username already exist");
                }
            }
            ViewBag.Status = new SelectList(ListContainer.UserStatus, "Key", "Value", support.Status);
            ViewBag.Facility = new SelectList(facilities, "Id", "Description", support.FacilityId);
            return PartialView("~/Views/Admin/UpdateSupport.cshtml", model);
        }

        #region HELPERS

        private async Task LoginAsAsync(int facilityId, string level)
        {
            var user = await _context.User.FindAsync(UserId);
            var properties = new AuthenticationProperties
            {
                AllowRefresh = false,
                IsPersistent = false
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.GivenName, user.Firstname+" "+user.Middlename),
                new Claim(ClaimTypes.Surname, user.Lastname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.Contact),
                new Claim(ClaimTypes.Role, level),
                new Claim("Facility", facilityId.ToString()),
                new Claim("Department", user.DepartmentId.ToString()),
                new Claim("Province", user.ProvinceId.ToString()),
                new Claim("Muncity", user.MuncityId.ToString()),
                new Claim("Barangay", user.BarangayId.ToString()),
                new Claim("RealRole", user.Level),
                new Claim("RealFacility", user.FacilityId.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal, properties);
        }


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
                Id = currentSupport.Id,
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
        private async Task<User> SetSupportViewModel(UpdateSupportViewModel model)
        {
            var support = await _context.User.FindAsync(model.Id);

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