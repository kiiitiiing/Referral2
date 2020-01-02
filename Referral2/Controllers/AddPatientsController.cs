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

namespace Referral2.Controllers
{
    [Authorize(Policy = "Doctor")]
    public class AddPatientsController : Controller
    {
        const string SessionPatientId = "_patient_id";
        private readonly ReferralDbContext _context;
        private readonly ResourceManager Roles = new ResourceManager("Referral2.Roles", Assembly.GetExecutingAssembly());
        private readonly ResourceManager Status = new ResourceManager("Referral2.ReferralStatus", Assembly.GetExecutingAssembly());

        public AddPatientsController(ReferralDbContext context)
        {
            _context = context;
        }

        //GET: Return View of Add Patient
        [HttpGet]
        public IActionResult Add()
        {
            SetUser();
            ViewBag.BarangayId = new SelectList(_context.Barangay.Where(x => x.ProvinceId.Equals(CurrentUser.user.ProvinceId)), "Id", "Description");
            ViewBag.MuncityId = new SelectList(_context.Muncity.Where(x => x.ProvinceId.Equals(CurrentUser.user.ProvinceId)), "Id", "Description");
            ViewBag.CivilStatus = new SelectList(ListContainer.CivilStatus);
            ViewBag.PhicStatus = new SelectList(ListContainer.PhicStatus);
            ViewBag.Sex = ListContainer.Sex;
            return View();
        }

        //POST: Save patient
        [HttpPost]
        public async Task<IActionResult> Add([Bind] Patient patient)
        {
            patient.ProvinceId = CurrentUser.user.ProvinceId;
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
            int facilityId = int.Parse(User.FindFirstValue("Facility"));
            var facility = _context.Facility;
            var patient = _context.Patient.Find(id);
            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            string currentUserName = User.FindFirstValue(ClaimTypes.GivenName) + " " + User.FindFirstValue(ClaimTypes.Surname);
            HttpContext.Session.SetInt32(SessionPatientId, id);

            ViewBag.ReferringFacility = facility.Find(facilityId).Name;
            ViewBag.ReferringFacilityAddress = facility.Find(facilityId).Address;
            ViewBag.ReferringMd = "Dr. " + currentUserName;
            ViewBag.ReferredTo = new SelectList(facility.Where(x => !x.Id.Equals(facilityId)), "Id", "Name");
            ViewBag.PatientName = patient.FirstName + " " + patient.MiddleName + " " + patient.LastName;
            ViewBag.PatientAge = DateTime.Now.Year - patient.DateOfBirth.Year;
            ViewBag.PatientSex = patient.Sex;
            ViewBag.PatientStatus = patient.CivilStatus;
            ViewBag.PatientAddress = patient.Barangay.Description + ", " + patient.Muncity.Description + ", " + patient.Province.Description;
            ViewBag.PatientPhilHealthStatus = patient.PhicStatus;
            ViewBag.PatientPhilHealthId = patient.PhicId;
            return PartialView();
        }

        //POst: ReferPartial Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refer([Bind] PatientFormViewModel model)
        {
            var id = (int)HttpContext.Session.GetInt32(SessionPatientId);
            var facility = _context.Facility.Find(int.Parse(User.FindFirstValue("Facility")));
            var currentPatient = _context.Patient.Find(id);
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
            ViewBag.ReferringMd = "Dr." + User.FindFirstValue(ClaimTypes.GivenName)+ " " + User.FindFirstValue(ClaimTypes.Surname);
            ViewBag.DepartmentId = new SelectList(_context.Department, "Id", "Description", model.DepartmentId);
            ViewBag.ReferredMd = new SelectList(_context.User, "Id", "Lastname", model.ReferredMd);
            ViewBag.PatientName = currentPatient.FirstName + " " + currentPatient.MiddleName + " " + currentPatient.LastName;
            ViewBag.PatientAge = DateTime.Now.Year - currentPatient.DateOfBirth.Year;
            ViewBag.PatientSex = currentPatient.Sex;
            ViewBag.PatientStatus = currentPatient.CivilStatus;
            ViewBag.PatientAddress = currentPatient.Barangay.Description + ", " + currentPatient.Muncity.Description + ", " + currentPatient.Province.Description;
            ViewBag.PatientPhilHealthStatus = currentPatient.PhicStatus;
            ViewBag.PatientPhilHealthId = currentPatient.PhicId;
            return PartialView(model);
        }



        #region Helpers

        public PatientForm setNormalPatientForm(PatientFormViewModel model)
        {
            int userFacilityId = int.Parse(User.FindFirstValue("Facility"));
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            PatientForm patient = new PatientForm
            {
                UniqueId = model.PatientId + "-" + userFacilityId + "-" + DateTime.Now.ToString("yyMMddhh"),
                Code = DateTime.Now.ToString("yyMMdd") + "-" + userFacilityId.ToString().PadLeft(3, '0') + "-" + DateTime.Now.ToString("hhmmss"),
                ReferringFacilityId = userFacilityId,
                ReferredTo = model.FacilityId,
                DepartmentId = model.DepartmentId,
                TimeReferred = DateTime.Now,
                TimeTransferred = default,
                PatientId = model.PatientId,
                CaseSummary = model.CaseSummary,
                RecommendSummary = model.SummaryReCo,
                Diagnosis = model.Diagnosis,
                Reason = model.Reason,
                ReferringMd = userId,
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
                Status = Status.GetString("REFERRED"),
                FormId = patientForm.Id,
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
                ActionMd = tracking.ActionMd,
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

        public void SetUser()
        {
            CurrentUser.user = _context.User.Find(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        }

        #endregion


    }
}