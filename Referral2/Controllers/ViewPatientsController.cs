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

namespace Referral2.Controllers
{
    [Authorize(Policy = "Doctor")]
    public class ViewPatientsController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly ICompute _compute;
        private readonly ResourceManager Status = new ResourceManager("Referral2.ReferralStatus", Assembly.GetExecutingAssembly());

        public ViewPatientsController(ReferralDbContext context, ICompute compute)
        {
            _context = context;
            _compute = compute;
        }

        public string Code { get; set; }

        //GET: Patients
        public async Task<IActionResult> ListPatients(string currentFilter, string searchString, int muncityId, int barangayId, int? pageNumber)
        {
            SetUser();
            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.Muncities = new SelectList(_context.Muncity.Where(x => x.ProvinceId.Equals(CurrentUser.user.ProvinceId)), "Id", "Description");
            ViewBag.CurrentFilter = searchString;

            var patients = from s in _context.Patient
                                    .Include(x => x.Barangay)
                                    .Include(x => x.Muncity)
                                    .Include(x => x.Province)
                                    .OrderByDescending(x => x.CreatedAt)
                           select s;

            if (!string.IsNullOrEmpty(searchString))
                patients = patients.Where(s => s.LastName.Contains(searchString) ||
                                                s.FirstName.Contains(searchString) ||
                                                s.MiddleName.Contains(searchString))
                                                .OrderByDescending(x => x.CreatedAt);

            int pageSize = 5;


            return View(await PaginatedList<Patient>.CreateAsync(patients.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        
        //GET: Accepted patients
        public async Task<IActionResult> Accepted(string searchString, string dateRange, string currentFilter, int? pageNumber)
        {
            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            DateTime startDate = default;
            DateTime endDate = default;

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
                               Code = t.Code,
                               Patient = t.Patient,
                               Status = t.Status,
                               DateAction = c.DateReferred,
                               ReferredFrom = t.ReferredFromNavigation,
                               ReferredTo = (int)t.ReferredTo,
                               Type = t.Type,
                               UpdatedAt = t.UpdatedAt,
                               ActionMd = c.ActionMdNavigation
                           };

            accepted = accepted.Where(x => x.ReferredTo.Equals(CurrentUser.user.FacilityId) && x.Status != Status.GetString("TRANSFERRED"));

            ViewBag.Total = accepted.Count();

            IQueryable<AcceptedViewModel> acceptedView = null;

            if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrEmpty(dateRange))
            {
                //input.Substring(input.LastIndexOf("/"));
                startDate = DateTime.Parse(dateRange.Substring(0, dateRange.IndexOf(" ") + 1).Trim());
                endDate = DateTime.Parse(dateRange.Substring(dateRange.LastIndexOf(" ")).Trim());
                acceptedView = accepted.Where(s => s.Code.Contains(searchString) || s.Patient.LastName.Contains(searchString) || s.Patient.FirstName.Contains(searchString))
                                        .Where(x => x.DateAction.Date >= startDate && x.DateAction.Date <= endDate);
                ViewBag.Total = acceptedView.Count();
            }
            else
            {
                acceptedView = accepted;
                ViewBag.Total = acceptedView.Count();
            }

            //ViewBag.CurrentUser = CurrentUser.user;

            int pageSize = 15;

            return View(await PaginatedList<AcceptedViewModel>.CreateAsync(acceptedView.OrderByDescending(x => x.DateAction).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //GET: Dischagred Patients
        public async Task<IActionResult> Discharged(string searchString, string currentFilter, int? pageNumber)
        {
            var currentUser = _context.User.Find(int.Parse(User.Claims.First(x => x.Type.Equals(ClaimTypes.NameIdentifier)).Value));

            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

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
                                Code = t.Code,
                                Patient = t.Patient,
                                Status = t.Status,
                                DateAction = c.DateReferred,
                                ReferredFrom = t.ReferredFromNavigation,
                                Type = t.Type,
                            };

            discharge = discharge.Where(x => x.Status.Equals(Status.GetString("DISCHARGED")) || x.Status.Equals(Status.GetString("TRANSFERRED")));


            ViewBag.Total = discharge.Count();

            if (!string.IsNullOrEmpty(searchString))
            {
                discharge = discharge.Where(s => s.Code.Contains(searchString) || s.Patient.LastName.Contains(searchString) || s.Patient.FirstName.Contains(searchString));
                ViewBag.Total = discharge.Count();
            }

            int pageSize = 15;

            return View(await PaginatedList<DischargedViewModel>.CreateAsync(discharge.OrderByDescending(x => x.DateAction).AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        //GET: Cancelled Patients
        public async Task<IActionResult> Cancelled(string searchString, string currentFilter, int? pageNumber)
        {
            var currentUser = _context.User.Find(int.Parse(User.Claims.First(x => x.Type.Equals(ClaimTypes.NameIdentifier)).Value));

            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var acceptedPatients = from s in _context.Activity
                                            .Include(i => i.Department)
                                            .Include(i => i.Patient)
                                            .Include(i => i.ReferredFromNavigation)
                                            .Include(i => i.ReferredToNavigation)
                                            .Where(s => s.ReferredTo == currentUser.FacilityId && (s.Status == "cancelled"))
                                   select s;

            ViewBag.Total = acceptedPatients.Count();

            if (!string.IsNullOrEmpty(searchString))
            {
                acceptedPatients = acceptedPatients.Where(s => s.Code.Contains(searchString) || s.Patient.LastName.Contains(searchString) || s.Patient.FirstName.Contains(searchString));
                ViewBag.Total = acceptedPatients.Count();
            }

            int pageSize = 15;

            return View(await PaginatedList<Activity>.CreateAsync(acceptedPatients.OrderByDescending(x => x.DateReferred).AsNoTracking(), pageNumber ?? 1, pageSize));
        }


        //GET: Archived patients
        public async Task<IActionResult> Archived(string searchString, string currentFilter, int? pageNumber)
        {
            var currentUser = _context.User.Find(int.Parse(User.Claims.First(x => x.Type.Equals(ClaimTypes.NameIdentifier)).Value));

            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var archivedPatients = from s in _context.Activity
                                            .Include(i => i.Department)
                                            .Include(i => i.Patient)
                                            .Include(i => i.ReferredFromNavigation)
                                            .Include(i => i.ReferredToNavigation)
                                            .Where(s => s.ReferredTo == currentUser.FacilityId && s.DateReferred >= s.DateReferred.AddDays(3))
                                            
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

        public IActionResult Test()
        {
            var tess = _context.Incoming.FromSqlRaw("EXEC [dbo].[incoming_procedure]").ToList();

            return View(tess);
        }


        //GET: Track patients
        public async Task<IActionResult> Track(string code)
        {

            ViewBag.CurrentCode = code;

            var track = from s in _context.Activity
                                    .Include(x => x.ReferredFromNavigation)
                                    .Include(x => x.ReferredToNavigation)
                                    .Include(x => x.ReferringMdNavigation)
                                        .ThenInclude(x => x.Facility)
                                    .Include(x => x.ActionMdNavigation)
                                        .ThenInclude(x => x.Facility)
                                    .Include(x => x.Patient)
                                    .Where(x => x.Code.Equals(code)).OrderByDescending(x => x.CreatedAt)
                        select s;


            if(track.Count() != 0)
            {
                var patient = track.Last().Patient;

                var referringMd = track.Last().ReferringMdNavigation;

                ViewBag.Patient = patient.FirstName + " " + patient.MiddleName + " " + patient.LastName;
                ViewBag.Sex = patient.Sex;
                ViewBag.Age = _compute.GetAge(patient.DateOfBirth);
                ViewBag.Address = patient.Barangay.Description + ", " + patient.Muncity.Description + ", " + patient.Province.Description;
                ViewBag.ReferredBy = "Dr. " + referringMd.Firstname + " " + referringMd.Middlename + " " + referringMd.Lastname;
                ViewBag.Code = track.First().Code;
            }

            return View(await track.ToListAsync());
        }


        //GET: Incoming patients
        public async Task<IActionResult> Incoming(string searhcString, string currentFilter, int? pageNumber)
        {
            var currentUser = _context.User.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

            if (searhcString != null)
                pageNumber = 1;
            else
                searhcString = currentFilter;


            ViewBag.CurrentFilter = searhcString;

            var incoming = from t in _context.Tracking.Where(f=>f.ReferredTo == currentUser.FacilityId)
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
                               Code = t.Code,
                               Patient = t.Patient,
                               Status = t.Status,
                               ReferringMd = t.ReferringMdNavigation,
                               ActionMd = t.ActionMdNavigation,
                               DateAction = c.DateReferred,
                               ReferredFrom = t.ReferredFromNavigation,
                               ReferredTo = t.ReferredToNavigation,
                               DepartmentId = t.Department
                           };

            incoming = incoming.Where(x => x.ReferredTo.Id == currentUser.FacilityId);


            ViewBag.Total = incoming.Count();

            if (!string.IsNullOrEmpty(searhcString))
            {
                incoming = incoming.Where(s => s.Code.Contains(searhcString));
                ViewBag.Total = incoming.Count();
            }

            int pageSize = 6;

            return View(await PaginatedList<IncomingViewModel>.CreateAsync(incoming.OrderByDescending(d => d.DateAction).AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        public async Task<IActionResult> Referred(string searchString, string currentFilter, int? pageNumber)
        {
            var currentUser = _context.User.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));

            if (searchString != null)
                pageNumber = 1;
            else
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var activities = _context.Activity;
            var referred = _context.Tracking.Where(x => x.ReferredFrom == currentUser.FacilityId)
                                            .Select(t => new ReferredViewModel
                                            {
                                                ReferredFrom = t.ReferredFromNavigation,
                                                ReferredTo = t.ReferredToNavigation,
                                                Department = t.Department,
                                                Code = t.Code,
                                                Patient = t.Patient,
                                                ReferringMd = t.ReferringMdNavigation,
                                                ActionMd = t.ActionMdNavigation,
                                                Activities = activities.Where(x=>x.Code.Equals(t.Code)).OrderByDescending(x=>x.UpdatedAt),
                                                UpdatedAt = t.UpdatedAt
                                            });


            ViewBag.Total = referred.Count();
            ViewBag.Muncities = new SelectList(_context.Muncity.Where(x=>x.ProvinceId.Equals(int.Parse(User.FindFirstValue("Province")))), "Id", "Description");

            if (!string.IsNullOrEmpty(searchString))
            {
                referred = referred.Where(s => s.Code.Contains(searchString));
                ViewBag.Total = referred.Count();
            }

            int pageSize = 6;

            return View(await referred.OrderByDescending(x=>x.UpdatedAt).ToListAsync());
        }

        #region HELPERS

        private void SetUser()
        {
            CurrentUser.user = _context.User.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        }


        #endregion
    }
}