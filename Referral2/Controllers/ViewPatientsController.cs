using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Referral2.Models.ViewModels.ViewPatients;
using Microsoft.Extensions.Options;
using Referral2.Models.ViewModels.Doctor;
using MoreLinq.Extensions;

namespace Referral2.Controllers
{
    //[Authorize(Policy = "Doctor")]
    public class ViewPatientsController : HomeController
    {
        public const string SessionKeySearch = "_search";
        public const string SessionKeyCode = "_code";
        public const string SessionKeyDateRange = "_dateRange";
        public const string SessionKeyStatus = "_status";
        public const string SessionKeyFacility = "_facility";
        public const string SessionKeyDepartment = "_department";
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public ViewPatientsController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
            : base(context, status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        public string Search { get; set; }
        public string DateRange { get; set; }
        public string Status { get; set; }
        public int? FacilityId { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //GET: List Patients
        public async Task<IActionResult> ListPatients(string currentFilter, string name, int? muncityId, int? barangayId, int? pageNumber)
        {
            if (name != null)
                pageNumber = 1;
            else
                name = currentFilter;

            var muncities = _context.Muncity.Where(x => x.ProvinceId.Equals(UserProvince));

            if (muncityId == null)
                ViewBag.Muncities = new SelectList(muncities, "Id", "Description");
            else
            {
                var barangays = _context.Barangay.Where(x => x.MuncityId.Equals(muncityId));
                ViewBag.Muncities = new SelectList(muncities, "Id", "Description", muncityId);
                ViewBag.Barangays = new SelectList(barangays, "Id", "Description", barangayId);
            }
                
            ViewBag.CurrentFilter = name;


            var patients = _context.Patient
            .Select(x => new PatientsViewModel
            {
                PatientId = x.Id,
                PatientName = x.FirstName + " " + x.MiddleName + " " + x.LastName,
                PatientSex = x.Sex,
                PatientAge = GlobalFunctions.ComputeAge(x.DateOfBirth),
                DateofBirth = x.DateOfBirth,
                MuncityId = x.MuncityId,
                Muncity = x.Muncity == null ? "" : x.Muncity.Description,
                BarangayId = x.BarangayId,
                Barangay = x.Barangay == null ? "" : x.Barangay.Description,
                CreatedAt = (DateTime)x.CreatedAt
            })
            .Where(x => x.PatientName.ToUpper().Contains(name) &&
            x.MuncityId.Equals(muncityId) && 
            x.BarangayId.Equals(barangayId));


            int pageSize = 5;


            return View(await PaginatedList<PatientsViewModel>.CreateAsync(patients.AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        //GET: Accepted patients
        public async Task<IActionResult> Accepted(string search, string dateRange, int? page)
        {
            #region Variable initialize
            ViewBag.CurrentSearch = search;

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            else
            {
                StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                EndDate = new DateTime(DateTime.Now.Year, 12, 31);
            }


            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");
            #endregion
            #region Query
            var accepted = _context.Tracking
               .Join( _context.Activity, t => new
                    {
                        Code = t.Code,
                        Status = t.Status
                    },
                a => new
                    {
                        Code = a.Code,
                        Status = a.Status
                    },
                (t, a) =>
                    new
                    {
                        t = t,
                        a = a
                    })
               .Where( x =>
                    x.t.ReferredTo == UserFacility && 
                    x.t.DepartmentId == UserDepartment &&
                    x.t.Status != _status.Value.DISCHARGED &&
                    x.t.Status != _status.Value.TRANSFERRED &&
                    x.t.Status != _status.Value.REFERRED &&
                    x.t.Status != _status.Value.CANCELLED &&
                    x.a.DateReferred >= StartDate &&
                    x.a.DateReferred <= EndDate
               )
               .Select( x => new AcceptedViewModel
                     {
                         ReferringFacility = x.t.ReferredFromNavigation.Name,
                         Type = x.t.Type,
                         PatientName = x.t.Patient.FirstName + " " + x.t.Patient.MiddleName + " " + x.t.Patient.LastName,
                         PatientCode = x.t.Code,
                         Status = x.t.Status,
                         DateAction = x.a.DateReferred,
                         ReferredTo = (int)x.t.ReferredTo,
                         ReferredToDepartment = (int)x.t.DepartmentId,
                         UpdatedAt = x.t.UpdatedAt
                     }
               );
            #endregion

            ViewBag.Total = accepted.Count();

            if (!string.IsNullOrEmpty(search))
            {
                accepted = accepted.Where(s => s.PatientCode.Equals(search) || s.PatientName.Contains(search));    
                ViewBag.Total = accepted.Count();
            }
            else
            {
                ViewBag.Total = accepted.Count();
            }


            int size = 3;

            return View(await PaginatedList<AcceptedViewModel>.CreateAsync(accepted.OrderByDescending(x => x.DateAction).AsNoTracking(), page ?? 1, size));
        }

        //GET: Dischagred Patients
        public async Task<IActionResult> Discharged(string search, string dateRange, int? page)
        {
            #region Variable initialize
            ViewBag.CurrentSearch = search;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");
            #endregion
            #region Query
            var discharge = _context.Tracking
                .Join(_context.Activity, t => new
                {
                    Code = t.Code,
                    Status = t.Status
                },
                a => new
                {
                    Code = a.Code,
                    Status = a.Status
                },
                (t, a) =>
                    new
                    {
                        t = t,
                        a = a
                    })
                .Where(x =>
                   x.t.ReferredTo == UserFacility &&
                   x.t.DepartmentId == UserDepartment &&
                   x.a.DateReferred >= StartDate &&
                   x.a.DateReferred <= EndDate &&
                   (x.t.Status == _status.Value.DISCHARGED ||
                   x.t.Status == _status.Value.TRANSFERRED)
                )
                .Select(x => new DischargedViewModel()
                    {
                        ReferringFacility = x.t.ReferredFromNavigation.Name,
                        Type = x.t.Type,
                        PatientName = x.t.Patient.FirstName + " " + x.t.Patient.MiddleName + " " + x.t.Patient.LastName,
                        Code = x.t.Code,
                        Status = x.t.Status,
                        DateAction = x.a.DateReferred
                    }
                );
            #endregion

            ViewBag.Total = discharge.Count();

            if (!string.IsNullOrEmpty(search))
            {
                discharge = discharge.Where(s => s.Code.Contains(search) || s.PatientName.Contains(search));
                ViewBag.Total = discharge.Count();
            }

            int size = 3;

            return View(await PaginatedList<DischargedViewModel>.CreateAsync(discharge.OrderByDescending(x => x.DateAction).AsNoTracking(), page ?? 1, size));
        }


        //GET: Cancelled Patients
        public async Task<IActionResult> Cancelled(string search, string dateRange, int? page)
        {
            #region Initialize variables
            ViewBag.CurrentSearch = search;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");
            #endregion
            #region Query
            var cancelled =_context.Tracking
                .Join(_context.Activity, t => new
                {
                    Code = t.Code,
                    Status = t.Status
                },
                a => new
                {
                    Code = a.Code,
                    Status = a.Status
                },
                (t, a) =>
                    new
                    {
                        t = t,
                        a = a
                    })
                .Where(x =>
                   x.t.ReferredTo.Equals(UserFacility) &&
                   x.t.Status.Equals(_status.Value.CANCELLED) &&
                   x.a.DateReferred >= StartDate &&
                   x.a.DateReferred <= EndDate
                )
                .Select(x => new CancelledViewModel()
                    {
                        ReferringFacilityId = (int)x.a.ReferredFrom,
                        ReferringFacility = x.t.ReferredFromNavigation.Name,
                        PatientType = x.t.Type,
                        PatientName = x.t.Patient.FirstName + " " + x.t.Patient.MiddleName + " " + x.t.Patient.LastName,
                        PatientCode = x.t.Code,
                        DateCancelled = x.a.DateReferred,
                        ReasonCancelled = x.a.Remarks,
                        Type = x.t.Type
                    }
                );
            #endregion

            if (!string.IsNullOrEmpty(search))
            {
                cancelled = cancelled.Where(s => s.PatientCode.Contains(search) || s.PatientName.Contains(search));
            }

            int size = 15;
            ViewBag.Total = cancelled.Count();
            return View(await PaginatedList<CancelledViewModel>.CreateAsync(cancelled.OrderByDescending(x => x.DateCancelled).AsNoTracking(), page ?? 1, size));
        }


        //GET: Archived patients
        public async Task<IActionResult> Archived(string search, string dateRange, int? page)
        {
            #region Initialize variables
            ViewBag.CurrentSearch = search;
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            else
            {
                StartDate = new DateTime(DateTime.Now.Year, 1, 1);
                EndDate = new DateTime(DateTime.Now.Year, 12, 31);
            }
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");
            #endregion
            #region Query
            var activities = _context.Activity
                .Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);
            var archivedPatients = _context.Tracking
                .Where(x=>
                    x.ReferredTo.Equals(UserFacility) && 
                    x.DepartmentId.Equals(UserDepartment))
                .Where(x=>
                    x.Status.Equals(_status.Value.REFERRED) ||
                    x.Status.Equals(_status.Value.SEEN))
               .Select(x => new ArchivedViewModel
               {
                   ReferringFacility = x.ReferredFromNavigation.Name,
                   Type = x.Type.Equals("pregnant"),
                   PatientName = GlobalFunctions.GetFullName(x.Patient),
                   Code = x.Code,
                   DateArchive = activities.FirstOrDefault(i=>i.Code.Equals(x.Code)).DateReferred,
                   Reason = activities.FirstOrDefault(i => i.Code.Equals(x.Code)).Remarks
               })
               .Where(x => DateTime.Now > x.DateArchive.AddDays(3));
            #endregion

            ViewBag.Total = archivedPatients.Count();

            if (!string.IsNullOrEmpty(search))
            {
                archivedPatients = archivedPatients.Where(s => s.Code.Equals(search) || s.PatientName.Contains(search));
                ViewBag.Total = archivedPatients.Count();
            }

            int pageSize = 15;

            return View(await PaginatedList<ArchivedViewModel>.CreateAsync(archivedPatients.OrderByDescending(x => x.DateArchive), page ?? 1, pageSize));

        }

        //GET: Track patients
        public async Task<IActionResult> Track(string code)
        {

            ViewBag.CurrentCode = code;
            var activities = _context.Activity.Where(x => x.Code.Equals(code));
            var feedbacks = _context.Feedback.Where(x => x.Code.Equals(code));

            var track = await _context.Tracking
                .Where(x => x.Code == code)
                .Select(t => new ReferredViewModel
                {
                    PatientId = t.PatientId,
                    PatientName = t.Patient.FirstName + " " + t.Patient.MiddleName + " " + t.Patient.LastName,
                    PatientSex = t.Patient.Sex,
                    PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                    PatientAddress = GlobalFunctions.GetAddress(t.Patient),
                    ReferredBy = GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                    ReferredTo = GlobalFunctions.GetMDFullName(t.ActionMdNavigation),
                    ReferredToId = t.ReferredTo,
                    TrackingId = t.Id,
                    SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                    CallerCount = activities.Where(x => x.Status.Equals(_status.Value.CALLING)).Count(),
                    ReCoCount = feedbacks.Where(x => x.Code.Equals(t.Code)).Count(),
                    Travel = string.IsNullOrEmpty(t.Transportation),
                    Code = t.Code,
                    Status = t.Status == _status.Value.REFERRED && t.DateSeen != default ? "seen" : t.Status,
                    Pregnant = t.Type.Equals("pregnant"),
                    Seen = t.DateSeen != default,
                    Walkin = t.WalkIn.Equals("yes"),
                    UpdatedAt = t.UpdatedAt,
                    Activities = activities.Where(x => x.Code.Equals(t.Code)).OrderByDescending(x => x.CreatedAt)
                        .Select(i => new ActivityLess
                        {
                            Status = i.Status,
                            DateAction = i.DateReferred.ToString("MMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture),
                            FacilityFrom = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Name,
                            FacilityFromContact = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Contact,
                            FacilityTo = t.ReferredToNavigation.Name,
                            PatientName = i.Patient.FirstName + " " + i.Patient.MiddleName + " " + i.Patient.LastName,
                            ActionMd = GlobalFunctions.GetMDFullName(i.ActionMdNavigation),
                            ReferringMd = GlobalFunctions.GetMDFullName(i.ReferringMdNavigation),
                            Remarks = i.Remarks
                        })
                })
                .FirstOrDefaultAsync();


            if(track != null)
            {
                ViewBag.Code = track.Code;
            }
            

            return View(track);
        }


        //GET: Incoming patients
        public async Task<IActionResult> Incoming(string search, string dateRange, int? department, string status,int? page)
        {
            #region Initialize variables
            ViewBag.CurrentSearch = search;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }

            var faciliyDepartment = _context.User
                .Where(x => x.FacilityId.Equals(UserFacility) && x.Level.Equals(_roles.Value.DOCTOR))
                .DistinctBy(x=>x.DepartmentId)
                .Select(x => new SelectDepartment
                {
                    DepartmentId = (int)x.DepartmentId,
                    DepartmentName = x.Department.Description
                });

            ViewBag.Departments = new SelectList(faciliyDepartment, "DepartmentId", "DepartmentName");
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");
            ViewBag.IncomingStatus = new SelectList(ListContainer.IncomingStatus, "Key", "Value");
            #endregion
            #region Query
            var activities = _context
                .Activity.Where(x => x.DateReferred >= StartDate && x.DateReferred <= EndDate);

            var incoming = _context.Tracking
                .Where(t => t.ReferredTo == UserFacility && t.DateReferred >= StartDate && t.DateReferred <= EndDate)
                .Select(t => new IncomingViewModel()
                {
                    Pregnant = t.Type.Equals("pregnant"),
                    TrackingId = t.Id,
                    Code = t.Code,
                    PatientName = GlobalFunctions.GetFullName(t.Patient),
                    PatientSex = t.Patient.Sex,
                    PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                    Status = t.Status,
                    ReferringMd = GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                    ActionMd = GlobalFunctions.GetMDFullName(activities.First(x => x.Status == t.Status && x.Code == t.Code).ActionMdNavigation),
                    SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                    CallCount = _context.Activity.Where(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.CALLING)).Count(),
                    FeedbackCount = _context.Feedback.Where(x => x.Code.Equals(t.Code)).Count(),
                    DateAction = activities.Where(x => x.Code == t.Code).OrderByDescending(x=>x.DateReferred).First().DateReferred,
                    ReferredFrom = t.ReferredFromNavigation.Name,
                    ReferredFromId = (int)t.ReferredFrom,
                    ReferredTo = t.ReferredToNavigation.Name,
                    ReferredToId = (int)t.ReferredTo,
                    Department = t.Department.Description,
                    DepartmentId = (int)t.DepartmentId
                });
            #endregion

            if (department != null)
            {
                incoming = incoming.Where(x => x.DepartmentId.Equals(department));
                ViewBag.Departments = new SelectList(faciliyDepartment, "DepartmentId", "DepartmentName", department);
                ViewBag.SelectedDepartment = department.ToString();
            }

            if (!string.IsNullOrEmpty(search))
            {
                incoming = incoming.Where(s => s.Code.Equals(search));
            }

            if(!string.IsNullOrEmpty(status))
            {
                if (status.Equals(_status.Value.ACCEPTED))
                    incoming = incoming.Where(x => x.Status != _status.Value.REFERRED);
                else
                    incoming = incoming.Where(x => x.Status.Equals(_status.Value.REFERRED));
                ViewBag.IncomingStatus = new SelectList(ListContainer.IncomingStatus, "Key", "Value", status);
                ViewBag.SelectedStatus = status;
            }

            int size = 3;

            ViewBag.Total = incoming.Count();
            return View(await PaginatedList<IncomingViewModel>.CreateAsync(incoming.OrderByDescending(x=>x.DateAction).AsNoTracking(), page ?? 1, size));
        }
        // GET: Referred
        public async Task<IActionResult> Referred(string search, string dateRange, int? facilityId, string status, int? page)
        {
            #region Initialize variables
            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            else
            {
                StartDate = new DateTime(DateTime.Now.Year, 1, 1, 0, 0, 0);
                EndDate = new DateTime(DateTime.Now.Year, 12, 31, 0, 0, 0);
            }

            ViewBag.StartDate = StartDate;
            ViewBag.EndDate = EndDate;
            #endregion
            #region Query
            var activities = _context.Activity.Where(x=>x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var feedbacks = _context.Feedback.Where(x => x.CreatedAt >= StartDate && x.CreatedAt <= EndDate);
            var facilities = _context.Facility.Where(x => x.Id != UserFacility);
            var referred = _context.Tracking
                .Where(x => x.ReferredFrom == UserFacility && x.DateReferred >= StartDate && x.DateReferred <= EndDate)
                .Select(t => new ReferredViewModel
                {
                    PatientId = t.PatientId,
                    PatientName = t.Patient.FirstName+" "+ t.Patient.MiddleName+" "+ t.Patient.LastName,
                    PatientSex = t.Patient.Sex,
                    PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                    PatientAddress = GlobalFunctions.GetAddress(t.Patient),
                    ReferredBy = GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                    ReferredTo = GlobalFunctions.GetMDFullName(t.ActionMdNavigation),
                    ReferredToId = t.ReferredTo,
                    TrackingId = t.Id,
                    SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                    CallerCount = activities.Where(x => x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.CALLING)).Count(),
                    IssueCount = _context.Issue.Where(x=>x.TrackingId.Equals(t.Id)).Count(),
                    ReCoCount = feedbacks.Where(x => x.Code.Equals(t.Code)).Count(),
                    Travel = string.IsNullOrEmpty(t.Transportation),
                    Code = t.Code,
                    Status = t.Status,
                    Pregnant = t.Type.Equals("pregnant"),
                    Seen = t.DateSeen != default,
                    Walkin = t.WalkIn.Equals("yes"),
                    UpdatedAt = t.UpdatedAt,
                    Activities = activities.Where(x => x.Code.Equals(t.Code)).OrderByDescending(x => x.CreatedAt)
                        .Select(i => new ActivityLess
                        {
                            Status = i.Status,
                            DateAction = i.DateReferred.ToString("MMM dd, yyyy hh:mm tt", CultureInfo.InvariantCulture),
                            FacilityFrom = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Name,
                            FacilityFromContact = i.ReferredFromNavigation == null ? "" : i.ReferredFromNavigation.Contact,
                            FacilityTo = t.ReferredToNavigation.Name,
                            PatientName = i.Patient.FirstName + " " + i.Patient.MiddleName + " " + i.Patient.LastName,
                            ActionMd = GlobalFunctions.GetMDFullName(i.ActionMdNavigation),
                            ReferringMd = GlobalFunctions.GetMDFullName(i.ReferringMdNavigation),
                            Remarks = i.Remarks
                        })
                });
            #endregion
            ViewBag.Total = referred.Count();
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            ViewBag.ListStatus = new SelectList(ListContainer.ListStatus, "Key", "Value");

            if (!string.IsNullOrEmpty(search))
            {
                referred = referred.Where(x => x.Code == search || x.PatientName.Contains(search));
                ViewBag.Total = referred.Count();
                ViewBag.CurrentSearch = search;
            }

            if(!string.IsNullOrEmpty(status))
            {
                referred = referred.Where(x => x.Status == status);
                ViewBag.Total = referred.Count();
                ViewBag.ListStatus = new SelectList(ListContainer.ListStatus, "Key", "Value", status);
                ViewBag.SelectedStatus = status;
            }

            if(facilityId != null)
            {
                referred = referred.Where(x => x.ReferredToId == facilityId);
                ViewBag.Total = referred.Count();
                ViewBag.Facilities = new SelectList(facilities, "Id", "Name", facilityId);
                ViewBag.SelectedFacility = facilityId.ToString();
            }

            int size = 3;

            return View(await PaginatedList< ReferredViewModel>.CreateAsync(referred.OrderByDescending(x=>x.UpdatedAt),page ?? 1, size));
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