using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Resources;
using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Referral2.Models;
using Referral2.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Referral2.Helpers;
using System.Security.Claims;
using Referral2.Models.ViewModels.Doctor;
using Microsoft.Extensions.Options;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Doctor")]
    public class AddPatientsController : Controller
    {
        const string SessionPatientId = "_patient_id";
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public AddPatientsController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        //GET: Return View of Add Patient
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.BarangayId = new SelectList(_context.Barangay.Where(x => x.ProvinceId.Equals(UserProvince())), "Id", "Description");
            ViewBag.MuncityId = new SelectList(_context.Muncity.Where(x => x.ProvinceId.Equals(UserProvince())), "Id", "Description");
            ViewBag.CivilStatus = new SelectList(ListContainer.CivilStatus);
            ViewBag.PhicStatus = new SelectList(ListContainer.PhicStatus);
            return View();
        }

        //POST: Save patient
        [HttpPost]
        public async Task<IActionResult> Add([Bind] Patient patient)
        {
            patient.ProvinceId = UserProvince();
            if (ModelState.IsValid)
            {
                setPatients(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListPatients", "ViewPatients");
            }
            ViewBag.BarangayId = new SelectList(_context.Barangay, "Id", "Description", patient.BarangayId);
            ViewBag.MuncityId = new SelectList(_context.Muncity, "Id", "Description", patient.MuncityId);
            ViewBag.ProvinceId = new SelectList(_context.Province, "Id", "Description", patient.ProvinceId);
            ViewBag.CivilStatus = new SelectList(ListContainer.CivilStatus);
            ViewBag.PhicStatus = new SelectList(ListContainer.PhicStatus);
            return View(patient);
        }


        //GET: ReferPartial
        public IActionResult Refer(int id)
        {
            var facility = _context.Facility.Single(x => x.Id.Equals(UserFacility()));
            var patient = _context.Patient.Find(id);

            var referModel = new ReferPatientViewModel
            {
                ReferringFacility = facility.Name,
                ReferringFacilityAddress = GlobalFunctions.GetAddress(facility),
                PatientId = patient.Id,
                PatientName = GlobalFunctions.GetFullName(patient),
                PatientAge = GlobalFunctions.ComputeAge(patient.DateOfBirth),
                PatientSex = patient.Sex,
                PatientCivilStatus = patient.CivilStatus,
                PatientAddress = GlobalFunctions.GetAddress(patient),
                PatientPhicStatus = patient.PhicStatus,
                PatientPhicId = patient.PhicId
            };
            return PartialView(referModel);
        }

        //POst: ReferPartial Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refer([Bind] PatientFormViewModel model)
        {
            var facility = _context.Facility.Find(UserFacility());
            var currentPatient = _context.Patient.Find(model.PatientId);
            if (ModelState.IsValid)
            {
                var patient = setNormalPatientForm(model);
                var tracking = setNormalTracking(patient);
                var activity = setNormalActivity(tracking);
                _context.Add(tracking);
                _context.Add(activity);
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListPatients","ViewPatients");
            }
            ViewBag.ReferringFacility = facility.Name;
            ViewBag.ReferringMd = UserName();
            ViewBag.DepartmentId = new SelectList(_context.Department, "Id", "Description", model.DepartmentId);
            ViewBag.ReferredMd = new SelectList(_context.User, "Id", "Lastname", model.ReferredMd);
            ViewBag.PatientId = currentPatient.Id;
            ViewBag.PatientName = GlobalFunctions.GetFullName(currentPatient);
            ViewBag.PatientAge = GlobalFunctions.ComputeAge(currentPatient.DateOfBirth);
            ViewBag.PatientSex = currentPatient.Sex;
            ViewBag.PatientStatus = currentPatient.CivilStatus;
            ViewBag.PatientAddress = GlobalFunctions.GetAddress(currentPatient);
            ViewBag.PatientPhilHealthStatus = currentPatient.PhicStatus;
            ViewBag.PatientPhilHealthId = currentPatient.PhicId;
            return PartialView(model);
        }



        #region Helpers

        public PatientForm setNormalPatientForm(PatientFormViewModel model)
        {
            PatientForm patient = new PatientForm
            {
                UniqueId = model.PatientId + "-" + UserFacility() + "-" + DateTime.Now.ToString("yyMMddhh"),
                Code = DateTime.Now.ToString("yyMMdd") + "-" + UserFacility().ToString().PadLeft(3, '0') + "-" + DateTime.Now.ToString("hhmmss"),
                ReferringFacilityId = UserFacility(),
                ReferredTo = model.FacilityId,
                DepartmentId = model.DepartmentId,
                TimeReferred = DateTime.Now,
                TimeTransferred = default,
                PatientId = model.PatientId,
                CaseSummary = model.CaseSummary,
                RecommendSummary = model.SummaryReCo,
                Diagnosis = model.Diagnosis,
                Reason = model.Reason,
                ReferringMd = UserId(),
                ReferredMd = model.ReferredMd,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now                
            };
            return patient;
        }

        public Tracking setNormalTracking(PatientForm patientForm)
        {
            Tracking tracking = new Tracking
            {
                Code = patientForm.Code,
                PatientId = patientForm.PatientId,
                DateReferred = patientForm.TimeReferred,
                DateTransferred = patientForm.TimeTransferred,
                DateAccepted = default,
                DateArrived = default,
                DateSeen = default,
                ReferredFrom = patientForm.ReferringFacilityId,
                ReferredTo = patientForm.ReferredTo,
                DepartmentId = patientForm.DepartmentId,
                ReferringMd = patientForm.ReferringMd,
                ActionMd = patientForm.ReferredMd,
                Remarks = patientForm.Reason,
                Type = "normal",
                Status = _status.Value.REFERRED,
                //FormId = patientForm.Id,
                WalkIn = "no",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return tracking;
        }

        public Activity setNormalActivity(Tracking tracking)
        {
            Activity activity = new Activity
            {
                Code = tracking.Code,
                PatientId = tracking.PatientId,
                DateReferred = tracking.DateReferred,
                DateSeen = tracking.DateSeen,
                ReferredFrom = tracking.ReferredFrom,
                ReferredTo = tracking.ReferredTo,
                DepartmentId = tracking.DepartmentId,
                ReferringMd = tracking.ReferringMd,
                ActionMd = null,
                Remarks = tracking.Remarks,
                Status = tracking.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return activity;
        }


        public void setPatients(Patient patients)
        {
            patients.FirstName = fixString(patients.FirstName);
            patients.MiddleName = fixString(patients.MiddleName);
            patients.LastName = fixString(patients.LastName);
            patients.CreatedAt = DateTime.Now;
            patients.UpdatedAt = DateTime.Now;

            if (patients.TsekapPatient == null)
                patients.TsekapPatient = 0;

            if (patients.Address == null)
                patients.Address = "None";

            if (patients.PhicStatus.Equals("None"))
                patients.PhicId = "None";
            patients.UniqueId = RemoveWhiteSpace(patients.FirstName.Trim()) + patients.MiddleName.Trim() + patients.LastName.Trim() + RemoveDash(patients.DateOfBirth.ToString("yyyy/MM/dd").Trim()) + patients.BarangayId.ToString().Trim();
            _context.Add(patients);
        }
        public string fixString(string name)
        {
            name = name.Trim().ToLower();

            name = name.First().ToString().ToUpper() + name.Substring(1);

            return name;
        }

        public string RemoveWhiteSpace(string input)
        {
            return new string(input.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        public string RemoveDash(string input)
        {
            return new string(input.Where(c => char.IsDigit(c)).ToArray());
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