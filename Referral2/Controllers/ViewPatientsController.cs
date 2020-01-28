using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
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

namespace Referral2.Controllers
{
    [Authorize(Policy = "Doctor")]
    public class ViewPatientsController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public ViewPatientsController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        public string SearchString { get; set; }
        public string DateRange { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        //GET: List Patients
        public async Task<IActionResult> ListPatients(string currentFilter, string name, int? muncityId, int? barangayId, int? pageNumber)
        {
            if (name != null)
                pageNumber = 1;
            else
                name = currentFilter;

            var muncities = _context.Muncity.Where(x => x.ProvinceId.Equals(UserProvince()));

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
        public async Task<IActionResult> Accepted(string searchString, string dateRange, string currentFilter, int? pageNumber)
        {
            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");

            var accepted = from t in _context.Tracking
                           join a in _context.Activity
                           on new
                           {
                               Code = t.Code,
                               Status = t.Status
                           }
                           equals new
                           {
                               Code = a.Code,
                               Status = a.Status
                           }
                           into tact
                           from c in tact.DefaultIfEmpty()
                           select new AcceptedViewModel()
                           {
                               ReferringFacility = t.ReferredFromNavigation.Name,
                               Type = t.Type,
                               PatientName = t.Patient.FirstName+" "+ t.Patient.MiddleName+" "+ t.Patient.LastName,
                               PatientCode = t.Code,
                               Status = t.Status,
                               DateAction = c.DateReferred,
                               ReferredTo = (int)t.ReferredTo,
                               ReferredToDepartment = (int)t.DepartmentId,
                               UpdatedAt = t.UpdatedAt
                           };

            accepted = accepted
                .Where(x => x.ReferredTo.Equals(UserFacility()) && x.ReferredToDepartment.Equals(UserDepartment()))
                .Where(x => x.Status != _status.Value.DISCHARGED)
                .Where(x => x.Status != _status.Value.TRANSFERRED)
                .Where(x => x.Status != _status.Value.REFERRED)
                .Where(x => x.Status != _status.Value.CANCELLED)
                .Where(x => x.DateAction.Date >= StartDate && x.DateAction.Date <= EndDate);

            ViewBag.Total = accepted.Count();

            if (!string.IsNullOrEmpty(searchString))
            {
                accepted = accepted.Where(s => s.PatientCode.Equals(searchString) || s.PatientName.Contains(searchString));    
                ViewBag.Total = accepted.Count();
            }
            else
            {
                ViewBag.Total = accepted.Count();
            }


            int pageSize = 15;

            return View(await PaginatedList<AcceptedViewModel>.CreateAsync(accepted.OrderByDescending(x => x.DateAction).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //GET: Dischagred Patients
        public async Task<IActionResult> Discharged(string searchString, string dateRange, string currentFilter, int? pageNumber)
        {
            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");

            var discharge = from t in _context.Tracking
                            join a in _context.Activity
                            on new
                            {
                                Code = t.Code,
                                Status = t.Status
                            }
                            equals new
                            {
                                Code = a.Code,
                                Status = a.Status
                            }
                            into tact
                            from c in tact.DefaultIfEmpty()
                            select new DischargedViewModel()
                            {
                                ReferringFacility = t.ReferredFromNavigation.Name,
                                Type = t.Type,
                                PatientName = t.Patient.FirstName+" "+ t.Patient.MiddleName+" "+ t.Patient.LastName,
                                Code = t.Code,
                                Status = t.Status,
                                DateAction = c.DateReferred
                            };

            discharge = discharge
                .Where(x => x.Status.Equals(_status.Value.DISCHARGED) || x.Status.Equals(_status.Value.TRANSFERRED))
                .Where(x => x.DateAction.Date >= StartDate && x.DateAction.Date <= EndDate);


            ViewBag.Total = discharge.Count();

            if (!string.IsNullOrEmpty(searchString))
            {
                discharge = discharge.Where(s => s.Code.Contains(searchString) || s.PatientName.Contains(searchString));
                ViewBag.Total = discharge.Count();
            }

            int pageSize = 15;

            return View(await PaginatedList<DischargedViewModel>.CreateAsync(discharge.OrderByDescending(x => x.DateAction).AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        //GET: Cancelled Patients
        public async Task<IActionResult> Cancelled(string searchString, string dateRange, string currentFilter, int? pageNumber)
        {
            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");

            var cancelled = from t in _context.Tracking.Where(x => x.Status.Equals(_status.Value.CANCELLED))
                            join a in _context.Activity.Where(x => x.Status.Equals(_status.Value.CANCELLED))
                            on t.Code equals a.Code
                            into tact
                            from c in tact.DefaultIfEmpty()
                            select new CancelledViewModel()
                            {
                                ReferringFacilityId = (int)c.ReferredFrom,
                                ReferringFacility = t.ReferredFromNavigation.Name,
                                PatientType = t.Type,
                                PatientName = t.Patient.FirstName+" "+ t.Patient.MiddleName+" "+ t.Patient.LastName,
                                PatientCode = t.Code,
                                DateCancelled = c.DateReferred,
                                ReasonCancelled = c.Remarks
                            };

            cancelled = cancelled
                .Where(x => x.ReferringFacilityId.Equals(UserFacility()))
                .Where(x => x.DateCancelled.Date >= StartDate && x.DateCancelled.Date <= EndDate);

            ViewBag.Total = cancelled.Count();

            if (!string.IsNullOrEmpty(searchString))
            {
                cancelled = cancelled.Where(s => s.PatientCode.Contains(searchString) || s.PatientName.Contains(searchString));
                ViewBag.Total = cancelled.Count();
            }

            int pageSize = 15;

            return View(await PaginatedList<CancelledViewModel>.CreateAsync(cancelled.OrderByDescending(x => x.DateCancelled).AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        //GET: Archived patients
        public async Task<IActionResult> Archived(string searchString, string dateRange, string currentFilter, int? pageNumber)
        {
            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");


            var archivedPatients = from s in _context.Activity
                                            .Include(i => i.Department)
                                            .Include(i => i.Patient)
                                            .Include(i => i.ReferredFromNavigation)
                                            .Include(i => i.ReferredToNavigation)
                                            .Where(s => s.ReferredTo == UserFacility() && s.DateReferred >= s.DateReferred.AddDays(3))
                                            
                                   select s;

            ViewBag.Total = archivedPatients.Count();

            if (!string.IsNullOrEmpty(searchString))
            {
                archivedPatients = archivedPatients.Where(s => s.Code.Contains(searchString) || s.Patient.LastName.Contains(searchString) || s.Patient.FirstName.Contains(searchString));
                ViewBag.Total = archivedPatients.Count();
            }

            int pageSize = 15;

            return View(await PaginatedList<Activity>.CreateAsync(archivedPatients.OrderByDescending(x => x.DateReferred).AsNoTracking(), pageNumber ?? 1, pageSize));

        }

        //GET: Track patients
        public async Task<IActionResult> Track(string code)
        {

            ViewBag.CurrentCode = code;
            var activities = _context.Activity.Where(x => x.Code.Equals(code));
            var feedbacks = _context.Feedback.Where(x => x.Code.Equals(code));

            var track = await _context.Tracking
                .Where(x => x.Code.Equals(code))
                .Select(t => new ReferredViewModel
                {
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
                    Activities = activities.OrderByDescending(x => x.CreatedAt),
                    UpdatedAt = t.UpdatedAt
                })
                .FirstOrDefaultAsync();


            if(track != null)
            {
                ViewBag.Code = track.Code;
            }
            

            return View(track);
        }


        //GET: Incoming patients
        public async Task<IActionResult> Incoming(string searchFilter, string currentFilter, string dateRange, int? departmentFilter, string statusFilter,int? pageNumber)
        {
            if (searchFilter != null)
                pageNumber = 1;
            else
                searchFilter = currentFilter;

            ViewBag.CurrentSearchFilter = searchFilter;

            StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            EndDate = new DateTime(DateTime.Now.Year, 12, 31);

            if (!string.IsNullOrEmpty(dateRange))
            {
                StartDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                EndDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
            }
            var departments = _context.Department.Select(x => new SelectDepartment
            {
                DepartmentId = x.Id,
                DepartmentName = x.Description
            });

            ViewBag.StartDate = StartDate.Date.ToString("yyyy/MM/dd");
            ViewBag.EndDate = EndDate.Date.ToString("yyyy/MM/dd");


            var incoming = from t in _context.Tracking.Where(f => f.ReferredTo == UserFacility())
                           join a in _context.Activity
                           on new
                           {
                               Code = t.Code,
                               Status = t.Status
                           }
                           equals new
                           {
                               Code = a.Code,
                               Status = a.Status
                           }
                           into tact
                           from c in tact.DefaultIfEmpty()
                           select new IncomingViewModel()
                           {
                               Pregnant = t.Type.Equals("pregnant"),
                               TrackingId = t.Id,
                               Code = t.Code,
                               PatientName = GlobalFunctions.GetFullName(t.Patient),
                               PatientSex = t.Patient.Sex,
                               PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                               Status = t.Status,
                               ReferringMd = GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                               ActionMd = GlobalFunctions.GetMDFullName(c.ActionMdNavigation),
                               SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                               CallCount = _context.Activity.Where(x=>x.Code.Equals(t.Code) && x.Status.Equals(_status.Value.CALLING)).Count(),
                               FeedbackCount = _context.Feedback.Where(x=>x.Code.Equals(t.Code)).Count(),
                               DateAction = c.DateReferred,
                               ReferredFrom = t.ReferredFromNavigation.Name,
                               ReferredFromId = (int)t.ReferredFrom,
                               ReferredTo = t.ReferredToNavigation.Name,
                               ReferredToId = (int)t.ReferredTo,
                               Department = t.Department.Description,
                               DepartmentId = (int)t.DepartmentId
                           };

            incoming = incoming
                .Where(x => x.ReferredToId == UserFacility())
                .Where(x => x.DateAction >= StartDate && x.DateAction <= EndDate);

            var faciliyDepartment = _context.User.Where(x => x.FacilityId.Equals(UserFacility()) && x.Level.Equals(_roles.Value.DOCTOR))
                                            .GroupBy(d => d.DepartmentId)
                                            .Select(y => new SelectDepartment
                                            {
                                                DepartmentId = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentId,
                                                DepartmentName = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentName
                                            });

            ViewBag.Total = incoming.Count();
            ViewBag.Departments = new SelectList(faciliyDepartment, "DepartmentId", "DepartmentName");

            if(departmentFilter!=null)
            {
                incoming = incoming.Where(x => x.DepartmentId.Equals(departmentFilter));
                ViewBag.Departments = new SelectList(faciliyDepartment, "DepartmentId", "DepartmentName", departmentFilter);
            }

            if (!string.IsNullOrEmpty(SearchString))
            {
                incoming = incoming.Where(s => s.Code.Equals(searchFilter));
                ViewBag.Total = incoming.Count();
            }

            int pageSize = 6;

            return View(await PaginatedList<IncomingViewModel>.CreateAsync(incoming.OrderByDescending(d => d.DateAction).AsNoTracking(), pageNumber ?? 1, pageSize));
        }
        // GET: Referred
        public async Task<IActionResult> Referred(string searchString, string currentFilter, int? pageNumber)
        {
            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var activities = _context.Activity;
            var feedbacks = _context.Feedback;

            var referred = _context.Tracking.Where(x => x.ReferredFrom == UserFacility())
                                            .Select(t => new ReferredViewModel
                                            {
                                                PatientName = GlobalFunctions.GetFullName(t.Patient),
                                                PatientSex = t.Patient.Sex,
                                                PatientAge = GlobalFunctions.ComputeAge(t.Patient.DateOfBirth),
                                                PatientAddress = GlobalFunctions.GetAddress(t.Patient),
                                                ReferredBy = GlobalFunctions.GetMDFullName(t.ReferringMdNavigation),
                                                ReferredTo = GlobalFunctions.GetMDFullName(t.ActionMdNavigation),
                                                TrackingId = t.Id,
                                                SeenCount = _context.Seen.Where(x => x.TrackingId.Equals(t.Id)).Count(),
                                                CallerCount = activities.Where(x => x.Status.Equals(_status.Value.CALLING)).Count(),
                                                ReCoCount = feedbacks.Where(x => x.Code.Equals(t.Code)).Count(),
                                                Travel = string.IsNullOrEmpty(t.Transportation),
                                                Code = t.Code,
                                                Status = t.Status,
                                                Pregnant = t.Type.Equals("pregnant"),
                                                Walkin = t.WalkIn.Equals("yes"),
                                                Activities = activities.Where(x => x.Code.Equals(t.Code)).OrderByDescending(x => x.CreatedAt),
                                                UpdatedAt = t.UpdatedAt
                                            }); ;

            ViewBag.Total = referred.Count();
            ViewBag.Facilities = new SelectList(_context.Facility.Where(x => x.Id != UserFacility()), "Id", "Name");
            ViewBag.Statuses = new SelectList(ListContainer.Status);

            if (!string.IsNullOrEmpty(searchString))
            {
                referred = referred.Where(s => s.Code.Contains(searchString));
                ViewBag.Total = referred.Count();
            }

            int pageSize = 6;

            return View(await referred.OrderByDescending(x=>x.UpdatedAt).ToListAsync());
        }

        #region HELPERS

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