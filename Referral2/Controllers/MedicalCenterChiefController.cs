using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models.ViewModels;
using Referral2.Models.ViewModels.Consolidated;
using Referral2.Models.ViewModels.Mcc;
using Referral2.Models.ViewModels.Support;
using Referral2.Models.ViewModels.ViewPatients;
using Referral2.Models;

namespace Referral2.Controllers
{
    [Authorize(Policy = "MCC")]
    public class MedicalCenterChiefController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public MedicalCenterChiefController(ReferralDbContext context, IOptions<ReferralStatus> status, IOptions<ReferralRoles> roles)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public partial class idk
        {
            public double mins { get; set; }
        }

        // DASHBOARD
        public IActionResult MccDashboard()
        {
            var activities = _context.Activity.Where(x => x.DateReferred.Year.Equals(DateTime.Now.Year));
            var totalDoctors = _context.User.Where(x => x.Level.Equals(_roles.Value.DOCTOR) && x.FacilityId.Equals(UserFacility)).Count();
            var onlineDoctors = _context.User.Where(x => x.LoginStatus.Equals("login") && x.Level.Equals(_roles.Value.DOCTOR)).Count();
            var referredPatients = _context.Tracking
                .Where(x => x.DateReferred != default || x.DateAccepted != default || x.DateArrived != default)
                .Where(x=>x.ReferredFrom.Equals(UserFacility)).Count();

            var adminDashboard = new SupportDashboadViewModel(totalDoctors, onlineDoctors, referredPatients);

            return View(adminDashboard);
        }

        public async Task<IActionResult> ConsolidatedMcc(string dateRange)
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
                .Where(x=>x.ReferredTo == UserFacility && x.DateAccepted != default && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
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
                    ItemName = i.Key == null? "" : GlobalFunctions.GetMDFullName(doctors.SingleOrDefault(x => x.Id == i.Key))
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
               .Take(10)
               .ToList();
               
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
               .Take(10)
               .ToList();
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
                    ItemName = departments.SingleOrDefault(x=>x.Id.Equals(i.Key)).Description
                })
               .ToListAsync();
            #endregion

            // --------------------- OUTGOING ---------------------
            #region OUTGOING
            // OUTGOING: OUTGOING
            var outOutgoing= await trackings
                .Where(x => x.ReferredFrom.Equals(UserFacility)).CountAsync();
            // OUTGOING: ACCEPTED
            var outAccepted = await trackings
                .Where(x => x.ReferredFrom == UserFacility && x.DateAccepted != default)
                .CountAsync();
            // OUTGOING: REDIRECTED
            var outRedirected = await activities
                .Where(x => x.ReferredFrom.Equals(UserFacility) && x.Status.Equals(_status.Value.REJECTED))
                .CountAsync();
            // OUTGOING: ARCHIVED
            var outArchived = await trackings
                .Where(x => x.ReferredFrom.Equals(UserFacility))
                .Where(x => x.Status.Equals(_status.Value.REFERRED) || x.Status.Equals(_status.Value.SEEN))
                .Where(x => DateTime.Now > x.DateReferred.AddDays(3))
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
               .Take(10)
               .ToList();

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
               .Take(10)
               .ToList();
            // OUTGOING: TRANSPORTATIONS
            var outTransportation = await trackings
                .Where(x=>x.ReferredFrom == UserFacility)
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

            var inAcceptance = trackings
                .Where(x => x.ReferredTo == UserFacility && x.DateAccepted != default)
                .Select(x => new
                {
                    mins = x.DateAccepted.Subtract(x.DateReferred).TotalMinutes
                }).ToList();

            var inAcc = inAcceptance.Count() > 0 ? inAcceptance.Average(x => x.mins) : 0;

            var inArrival = trackings
                .Where(x => x.ReferredTo == UserFacility && x.DateArrived != default)
                .Select(x => new
                {
                    mins = x.DateArrived.Subtract(x.DateReferred).TotalMinutes
                }).ToList();

            var inArr = inArrival.Count() > 0 ? inArrival.Average(x => x.mins) : 0;

            var consolidated = new ConsolidatedViewModel
            {
                FacilityLogo = facilities.Find(UserFacility).Picture,
                FacilityName = facilities.Find(UserFacility).Name,
                InIncoming = inIncoming,
                InAccepted = inAccepted,
                InViewed = inIncoming - inAccepted,
                InReferringFacilities = inReferringFac,
                InReferringDoctors = inReferringDoc,
                InDiagnosis = inDiagnosis,
                InReason = inReason,
                InTransportation = inTransportation,
                InDepartment = inDepartment,
                InRemarks = null,
                InAcceptance = inAcc,
                InArrival = inArr,
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


        // ONLINE USERS PER DEPARTMENT
        public async Task<IActionResult> OnlineUsersDepartment(string dateRange)
        {
            SetDates(dateRange);
            ViewBag.StartDate = StartDate.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.ToString("yyyy/MM/dd");

            var departments = _context.Department;
            var noUser = _context.User.Where(x => x.FacilityId.Equals(UserFacility));
            var users = await _context.User
                .Where(x => x.FacilityId.Equals(UserFacility))
                .GroupBy(x => x.DepartmentId)
                .Select(x => new UserDepartmentViewModel
                {
                    Department = departments.Single(y => y.Id.Equals(x.Key)).Description,
                    OnDuty = noUser.Where(y => y.DepartmentId.Equals(x.Key) && y.LoginStatus.Equals("login")).Where(y => y.LastLogin >= StartDate && y.LastLogin <= EndDate).Count(),
                    OffDuty = noUser.Where(y => y.DepartmentId.Equals(x.Key) && y.LoginStatus.Equals("login off")).Where(y => y.LastLogin >= StartDate && y.LastLogin <= EndDate).Count(),
                    NumberOfUsers = noUser.Where(y => y.DepartmentId.Equals(x.Key)).Count()
                })
                .AsNoTracking()
                .ToListAsync();

            return View(users);
        }


        // INCOMING
        public async Task<IActionResult> MccIncoming(int? page, string dateRange)
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

            var trackings = _context.Tracking
                .Where(x => x.ReferredTo.Equals(UserFacility) && x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var activities = _context.Activity
                .Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);

            var facilities = _context.Facility
                .Select(i => new MccIncomingViewModel
                {
                    Facility = i.Name,
                    AcceptedCount = trackings
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
                                    .Where(x => x.a.Status == _status.Value.ACCEPTED)
                                    .Count(),
                    RedirectedCount = trackings
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
                                    .Where(x => x.a.Status == _status.Value.REJECTED)
                                    .Count(),
                    SeenCount = trackings
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
                                    .Where(x => x.a.Status == _status.Value.SEEN)
                                    .Count(),
                    Total = trackings
                                    .Where(x => x.ReferredFrom == i.Id)
                                    .Count(),
                })
                .AsNoTracking();

            int size = 20;

            return View(await PaginatedList<MccIncomingViewModel>.CreateAsync(facilities.AsNoTracking(), page ?? 1, size));
        }


        public async Task<IActionResult> TimeFrame(string dateRange)
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

            var timeFrame = await _context.Tracking
                .Where(x => x.ReferredTo == UserFacility && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Select(x => new TimeFrameViewModel
                {
                    Code = x.Code,
                    TimeReferred = x.DateReferred.ToString("MMM d, yyyy h:mm tt", CultureInfo.InvariantCulture),
                    Seen = x.DateSeen == default ? 0 : x.DateSeen.Subtract(x.DateReferred).TotalMinutes,
                    Accepted = x.DateAccepted == default ? 0 : x.DateAccepted.Subtract(x.DateReferred).TotalMinutes,
                    Arrived = x.DateArrived == default ? 0 : x.DateArrived.Subtract(x.DateReferred).TotalMinutes,
                    Redirected = x.DateTransferred == default ? 0 : x.DateTransferred.Subtract(x.DateReferred).TotalMinutes
                })
                .ToListAsync();
            return View(timeFrame);

        }

        public async Task<IActionResult> Track(string code)
        {

            ViewBag.CurrentCode = code;
            var activities = _context.Activity.Where(x => x.Code.Equals(code));
            var feedbacks = _context.Feedback.Where(x => x.Code.Equals(code));

            var track = await _context.Tracking
                .Where(x => x.Code.Equals(code))
                .Select(t => new ReferredViewModel
                {
                    Pregnant = t.Type.Equals("pregnant"),
                    Seen = t.DateSeen != default,
                    TrackingId = t.Id,
                    PatientName = t.Patient.FirstName + " " + t.Patient.MiddleName + " " + t.Patient.LastName,
                    PatientSex = t.Patient.Sex,
                    PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                    PatientAddress = GlobalFunctions.GetAddress(t.Patient),
                    ReferredBy = t.ReferringMdNavigation == null ? "" : GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                    ReferredTo = t.ActionMdNavigation == null ? "" : GlobalFunctions.GetMDFullName(t.ActionMdNavigation),
                    SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                    CallerCount = activities == null ? 0 : activities.Where(x => x.Status.Equals(_status.Value.CALLING)).Count(),
                    ReCoCount = feedbacks == null ? 0 : feedbacks.Count(),
                    Travel = string.IsNullOrEmpty(t.Transportation),
                    Code = t.Code,
                    Status = t.Status,
                    Walkin = t.WalkIn.Equals("yes"),
                    UpdatedAt = t.UpdatedAt,
                    Activities = activities.Where(x => x.Code == t.Code).OrderByDescending(x => x.CreatedAt)
                        .Select(i => new ActivityLess
                        {
                            Status = i.Status,
                            DateAction = i.DateReferred.ToString("MMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture),
                            FacilityFrom = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Name,
                            FacilityFromContact = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Contact,
                            FacilityTo = i.ReferredToNavigation == null ? "" : i.ReferredToNavigation.Name,
                            PatientName = GlobalFunctions.GetFullName(i.Patient),
                            ActionMd = GlobalFunctions.GetMDFullName(i.ActionMdNavigation),
                            ReferringMd = GlobalFunctions.GetMDFullName(i.ReferringMdNavigation),
                            Remarks = i.Remarks
                        })
                })
                .FirstOrDefaultAsync();

            if (track != null)
            {
                ViewBag.Code = track.Code;
            }


            return View(track);
        }

        public async Task<IActionResult> OnlineDoctors(string name, int? facility)
        {
            ViewBag.CurrentSearch = name;
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.ProvinceId.Equals(UserProvince)), "Id", "Name");
            var onlineUsers = await _context.User.Where(x => x.LoginStatus.Contains("login") && x.LastLogin.Date.Equals(DateTime.Now.Date) && x.FacilityId.Equals(UserFacility)).ToListAsync();

            if (!string.IsNullOrEmpty(name))
            {
                onlineUsers.Where(x => x.Firstname.Contains(name) || x.Middlename.Contains(name) || x.Lastname.Contains(name));
            }
            if (facility != 0)
            {
                onlineUsers.Where(x => x.FacilityId.Equals(facility));
            }

            return View(onlineUsers);
        }

        #region HELPERS

        public void SetDates(string dateRange)
        {
            var lastDate = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
            StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, lastDate);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
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