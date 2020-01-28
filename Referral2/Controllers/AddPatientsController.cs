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
using Referral2.Models.ViewModels;

namespace Referral2.Controllers
{
    [Authorize(Policy = "Doctor")]
    public class AddPatientsController : Controller
    {
        const string SessionPatientId = "_patient_id";
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;
        private readonly IOptions<TrackingType> _type;

        // Add Patient Constructor
        public AddPatientsController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status, IOptions<TrackingType> type)
        {
            _context = context;
            _roles = roles;
            _status = status;
            _type = type;
        }

        #region Add Patient
        //GET: Add patient
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.BarangayId = new SelectList(_context.Barangay.Where(x => x.ProvinceId.Equals(UserProvince())), "Id", "Description");
            ViewBag.MuncityId = new SelectList(_context.Muncity.Where(x => x.ProvinceId.Equals(UserProvince())), "Id", "Description");
            ViewBag.CivilStatus = new SelectList(ListContainer.CivilStatus);
            ViewBag.PhicStatus = new SelectList(ListContainer.PhicStatus);
            return View();
        }

        //POST: Add patient
        [HttpPost]
        public async Task<IActionResult> Add([Bind] Patient patient)
        {
            patient.ProvinceId = UserProvince();
            if (ModelState.IsValid)
            {
                setPatients(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListPatients", "ViewPatients", new {name = patient.FirstName, muncityId = patient.MuncityId, barangayId = patient.BarangayId });
            }
            ViewBag.BarangayId = new SelectList(_context.Barangay, "Id", "Description", patient.BarangayId);
            ViewBag.MuncityId = new SelectList(_context.Muncity, "Id", "Description", patient.MuncityId);
            ViewBag.ProvinceId = new SelectList(_context.Province, "Id", "Description", patient.ProvinceId);
            ViewBag.CivilStatus = new SelectList(ListContainer.CivilStatus, patient.CivilStatus);
            ViewBag.PhicStatus = new SelectList(ListContainer.PhicStatus, patient.PhicStatus);
            return View(patient);
        }
        #endregion

        #region Refer Patient
        //GET: ReferPartial
        public IActionResult Refer(int? id)
        {
            var facility = _context.Facility.Single(x => x.Id.Equals(UserFacility()));
            var patient = _context.Patient.Find(id);
            var patientView = new PatientViewModel
            {
                Id = patient.Id,
                Name = GlobalFunctions.GetFullName(patient),
                Age = GlobalFunctions.ComputeAge(patient.DateOfBirth),
                Sex = patient.Sex,
                CivilStatus = patient.CivilStatus,
                Address = GlobalFunctions.GetAddress(patient),
                PhicStatus = patient.PhicStatus,
                PhicId = patient.PhicId
            };
            var referTo = _context.Facility
                .Where(x => x.Id != UserFacility())
                .Where(x => x.ProvinceId.Equals(UserProvince()));

            ViewBag.ReferredTo = new SelectList(referTo, "Id", "Name");
            ViewBag.Patient = patientView;
            ViewBag.Facility = facility.Name;
            ViewBag.FacilityAddress = GlobalFunctions.GetAddress(facility);

            return PartialView();
        }



        //POst: ReferPartial Post
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refer([Bind] ReferPatientViewModel model)
        {
            var currentPatient = _context.Patient.Find(model.PatientId);
            model.ReferredToMd = model.ReferredToMd == 0 ? null : model.ReferredToMd;
            if (ModelState.IsValid && model.ReferredTo != 0 && model.Department != 0)
            {
                var patient = setNormalPatientForm(model);
                var tracking = setNormalTracking(patient);
                var activity = SetNormalActivity(tracking);
                _context.Add(tracking);
                _context.Add(activity);
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListPatients", "ViewPatients");
            }
            else
            {
                ModelState.AddModelError("ReferredTo", "Please select a Facility.");
                ModelState.AddModelError("Department", "Please select a Department.");
            }
            var facility = _context.Facility.Single(x => x.Id.Equals(UserFacility()));
            var patientView = new PatientViewModel
            {
                Id = currentPatient.Id,
                Name = GlobalFunctions.GetFullName(currentPatient),
                Age = GlobalFunctions.ComputeAge(currentPatient.DateOfBirth),
                Sex = currentPatient.Sex,
                CivilStatus = currentPatient.CivilStatus,
                Address = GlobalFunctions.GetAddress(currentPatient),
                PhicStatus = currentPatient.PhicStatus,
                PhicId = currentPatient.PhicId
            };
            var referTo = _context.Facility
                .Where(x => x.Id != UserFacility())
                .Where(x => x.ProvinceId.Equals(UserProvince()));

            ViewBag.ReferredTo = new SelectList(referTo, "Id", "Name", model.ReferredTo);
            ViewBag.Patient = patientView;
            ViewBag.Facility = facility.Name;
            ViewBag.FacilityAddress = GlobalFunctions.GetAddress(facility);

            return PartialView(model);
        }
        #endregion

        #region Walkin Patient
        //GET: Walkin patient
        public async Task<IActionResult> Walkin(int? id)
        {
            var facility = _context.Facility.Find(UserFacility());
            var faciliyDepartment = await AvailableDepartments(UserFacility());
            var cuurentPatient = await SetPatient(id);
            var facilities = _context.Facility
                .Where(x => x.Id != UserFacility())
                .Where(x => x.ProvinceId.Equals(UserProvince()));
            ViewBag.Facility = facility.Name;
            ViewBag.FacilityAddress = GlobalFunctions.GetAddress(facility);
            ViewBag.Patient = cuurentPatient;
            ViewBag.ReferringFacility = new SelectList(facilities, "Id", "Name");
            ViewBag.Departments = new SelectList(faciliyDepartment, "DepartmentId", "DepartmentName");

            return PartialView();
        }
        //POST: Walkin Patient
        [HttpPost]
        public async Task<IActionResult> Walkin([Bind] WalkinPatientViewModel model)
        {
            model.ReferredToMd = model.ReferredToMd == 0 ? null : model.ReferredToMd;
            if (ModelState.IsValid && model.ReferringFacility != 0 && model.Department != 0)
            {
                var patient = setNormalPatientForm(model);
                var tracking = setWalkinTracking(patient);
                var activityReferred = SetReferredActivity(tracking);
                var activityAccepted = SetWalkinActivity(tracking);
                _context.Add(tracking);
                _context.Add(activityAccepted);
                _context.Add(activityReferred);
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListPatients", "ViewPatients");
            }
            else
            {
                ModelState.AddModelError("ReferringFacility", "Please select a Facility.");
                ModelState.AddModelError("Department", "Please Select a Department");
            }
            var walkin = await SetPatient(model.PatientId);
            var faciliyDepartment = await AvailableDepartments(UserFacility());
            var facility = _context.Facility.Find(UserFacility());
            var facilities = _context.Facility
                .Where(x => x.Id != UserFacility())
                .Where(x => x.ProvinceId.Equals(UserProvince()));
            ViewBag.Patient = walkin;
            ViewBag.Facility = facility.Name;
            ViewBag.FacilityAddress = GlobalFunctions.GetAddress(facility);
            ViewBag.ReferringFacility = new SelectList(facilities, "Id", "Name", model.ReferringFacility);
            ViewBag.Departments = new SelectList(faciliyDepartment, "DepartmentId", "DepartmentName");
            return PartialView(model);
        }
        #endregion

        #region Pregnant Refer Patient
        //GET: Pregnant Refer
        public async Task<IActionResult> PregnantRefer(int? id)
        {
            var patient = await SetPatient(id);
            var facilities = _context.Facility
                .Where(x => x.Id != UserFacility() && x.ProvinceId == UserProvince());
            ViewBag.Patient = patient;
            ViewBag.Facility = _context.Facility.Find(UserFacility()).Name;
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            return PartialView();
        }
        //POST: Pregnant Refer
        [HttpPost]
        public async Task<IActionResult> PregnantRefer([Bind] PregnantReferViewModel model)
        {
            var patient = await SetPatient(model.PatientId);
            var facilities = _context.Facility
                .Where(x => x.Id != UserFacility() && x.ProvinceId == UserProvince());
            if (ModelState.IsValid)
            {
                var pregnantForm = await SetPregnantForm(model, _type.Value.REFER);
                var tracking = await PregnantTracking(pregnantForm, _type.Value.REFER);
                var activity = await PregnantActivity(tracking, _type.Value.REFER);
                await _context.AddAsync(tracking);
                await _context.AddAsync(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListPatients", "ViewPatients");
            }
            else
            {
                ModelState.AddModelError("ReferredTo", "Please select a Facility.");
                ModelState.AddModelError("Department", "Please select a Department.");
                ModelState.AddModelError("WomanMajorFindings", "This field is required.");
                ModelState.AddModelError("WomanInformationGiven", "This field is required.");
            }
            ViewBag.Patient = patient;
            ViewBag.Facility = _context.Facility.Find(UserId()).Name;
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            return PartialView();
        }
        #endregion

        #region Pregnant Walkin Patient
        //GET: Pregnant Walkin
        public async Task<IActionResult> PregnantWalkin(int? id)
        {
            var patient = await SetPatient(id);
            var facility = _context.Facility.Find(UserFacility());
            var facilityDepartment = await AvailableDepartments(UserFacility());
            var facilities = _context.Facility
                .Where(x => x.Id != UserFacility() && x.ProvinceId == UserProvince());
            ViewBag.Patient = patient;
            ViewBag.Facility = facility.Name;
            ViewBag.FacilityAddress = GlobalFunctions.GetAddress(facility);
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            ViewBag.Departments = new SelectList(facilityDepartment, "DepartmentId", "DepartmentName");
            return PartialView();
        }
        //POST: Pregnant Walkin
        [HttpPost]
        public async Task<IActionResult> PregnantWalkin([Bind]PregnantReferViewModel model)
        {
            var patient = await SetPatient(model.PatientId);
            var facilities = _context.Facility
                .Where(x => x.Id != UserFacility() && x.ProvinceId == UserProvince());
            if (ModelState.IsValid)
            {
                var pregnantForm = await SetPregnantForm(model,_type.Value.WALKIN);
                var tracking = await PregnantTracking(pregnantForm, _type.Value.WALKIN);
                var activity = await PregnantActivity(tracking, _type.Value.WALKIN);
                await _context.AddAsync(tracking);
                await _context.AddAsync(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("ListPatients", "ViewPatients");
            }
            else
            {
                ModelState.AddModelError("ReferredTo", "Please select a Facility.");
                ModelState.AddModelError("Department", "Please select a Department.");
                ModelState.AddModelError("WomanMajorFindings", "This field is required.");
                ModelState.AddModelError("WomanInformationGiven", "This field is required.");
            }
            ViewBag.Patient = patient;
            ViewBag.Facility = _context.Facility.Find(UserId()).Name;
            ViewBag.Facilities = new SelectList(facilities, "Id", "Name");
            return PartialView();
        }
        #endregion

        #region Helpers
        private async Task<Activity> PregnantActivity(Tracking model, string type)
        {
            var walkin = type.Equals(_type.Value.WALKIN);
            if(walkin)
            {
                var referredActivity = new Activity
                {
                    Code = model.Code,
                    PatientId = model.PatientId,
                    DateReferred = model.DateReferred,
                    DateSeen = model.DateSeen,
                    ReferredFrom = model.ReferredFrom,
                    ReferredTo = model.ReferredTo,
                    DepartmentId = model.DepartmentId,
                    ReferringMd = model.ReferringMd,
                    ActionMd = null,
                    Status = _status.Value.REFERRED,
                    Remarks = model.Remarks,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                await _context.AddAsync(referredActivity);
            }
            var activity = new Activity
            {
                Code = model.Code,
                PatientId = model.PatientId,
                DateReferred = model.DateReferred,
                DateSeen = model.DateSeen,
                ReferredFrom = model.ReferredFrom,
                ReferredTo = model.ReferredTo,
                DepartmentId = model.DepartmentId,
                ReferringMd = model.ReferringMd,
                ActionMd = model.ActionMd,
                Status = model.Status,
                Remarks = walkin? "Walk-In Patient" : model.Remarks,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return activity;
        }
        private Task<Tracking> PregnantTracking(PregnantForm model, string type)
        {
            var walkin = type.Equals(_type.Value.WALKIN);
            var tracking = new Tracking
            {
                Code = model.Code,
                PatientId = model.PatientWomanId,
                DateReferred = model.ReferredDate,
                DateTransferred = default,
                DateAccepted = default,
                DateArrived = default,
                DateSeen = default,
                ReferredFrom = model.ReferringFacility,
                ReferredTo = model.ReferredTo,
                DepartmentId = model.DepartmentId,
                ReferringMd = model.ReferredBy,
                ActionMd = null,
                Status = walkin? _status.Value.ACCEPTED : _status.Value.REFERRED,
                Type = _type.Value.PREGNANT,
                WalkIn = walkin ? "yes" : "no",
                FormId = model.Id,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return Task.FromResult(tracking);
        }
        private async Task<Patient> SetBabyPatient(PregnantReferViewModel model, Patient mother)
        {
            var baby = new Patient
            {
                UniqueId = RemoveWhiteSpace(model.BabyFirstName + model.BabyMiddleName + model.BabyLastName) + RemoveDash(((DateTime)model.BabyDateOfBirth).ToString("yyyy/MM/dd")) + mother.BarangayId.ToString(),
                FirstName = model.BabyFirstName.Trim(),
                MiddleName = model.BabyMiddleName.Trim(),
                LastName = model.BabyLastName.Trim(),
                DateOfBirth = (DateTime)model.BabyDateOfBirth,
                Sex = "",
                CivilStatus = "Single",
                PhicStatus = null,
                PhicId = "None",
                BarangayId = mother.BarangayId,
                MuncityId = mother.MuncityId,
                ProvinceId = mother.ProvinceId,
                Address = GetAddress(mother),
                TsekapPatient = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _context.AddAsync(baby);
            await _context.SaveChangesAsync();
            return baby;
        }
        private async Task<PregnantForm> SetPregnantForm(PregnantReferViewModel model, string type)
        {
            var walkin = type.Equals(_type.Value.WALKIN);
            Patient mother = _context.Patient.Find(model.PatientId);
            Patient baby = null;
            if(!string.IsNullOrEmpty(model.BabyFirstName))
            {
                baby = await SetBabyPatient(model, mother);
            }
            var pregnantForm = new PregnantForm
            {
                UniqueId = model.PatientId + "-" + UserFacility() + "-" + DateTime.Now.ToString("yyMMddhh"),
                Code = DateTime.Now.ToString("yyMMdd") + "-" + UserFacility().ToString().PadLeft(3, '0') + "-" + DateTime.Now.ToString("hhmmss"),
                ReferringFacility = walkin? model.ReferredTo : UserFacility(),
                ReferredTo = walkin? UserFacility() : model.ReferredTo,
                ReferredBy = UserId(),
                RecordNo = model.RecordNumber?? "",
                ReferredDate = DateTime.Now,
                DepartmentId = model.Department,
                ArrivalDate = default,
                HealthWorker = model.HealthWorker?? "",
                PatientWomanId = model.PatientId,
                WomanReason = model.WomanMainReason,
                WomanMajorFindings = model.WomanMajorFindings,
                WomanBeforeTreatment = model.WomanBeforeTreatmentGiven,
                WomanBeforeGivenTime = model.WomanBeforeDateTimeGiven ?? default,
                WomanDuringTransport = model.WomanDuringTreatmentGiven,
                WomanTransportGivenTime = model.WomanDuringDateTimeGiven?? default,
                WomanInformationGiven = model.WomanInformationGiven,
                PatientBabyId = baby == null? null : (int?)baby.Id,
                BabyReason = model.BabyMainReason,
                BabyMajorFindings = model.BabyMajorFindings,
                BabyLastFeed = model.BabyLastFeed?? default,
                BabyBeforeTreatment = model.BabyBeforeTreatmentGiven,
                BabyBeforeGivenTime = model.BabyBeforeDateTimeGiven?? default,
                BabyDuringTransport = model.BabyDuringTreatmentGiven,
                BabyTransportGivenTime = model.BabyDuringDateTimeGiven?? default,
                BabyInformationGiven = model.BabyInformationGiven,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            var babyMother = new Baby
            {
                BabyId = baby == null? null : (int?)baby.Id,
                MotherId = model.PatientId,
                Weight = model.BabyBirthWeight ?? default,
                GestationalAge = model.GestationalAge ?? default,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _context.AddAsync(babyMother);
            await _context.AddAsync(pregnantForm);
            await _context.SaveChangesAsync();
            return pregnantForm;
        }
        private Task<PatientViewModel> SetPatient(int? id)
        {
            var patient = _context.Patient.Find(id);

            var patientModel = new PatientViewModel
            {
                Id = patient.Id,
                Name = GlobalFunctions.GetFullName(patient),
                Age = GlobalFunctions.ComputeAge(patient.DateOfBirth),
                Sex = patient.Sex,
                CivilStatus = patient.CivilStatus,
                Address = GlobalFunctions.GetAddress(patient),
                PhicStatus = patient.PhicStatus,
                PhicId = patient.PhicId
            };
            return Task.FromResult(patientModel);
        }
        private Task<IQueryable<SelectDepartment>> AvailableDepartments(int facilityId)
        {
            var departments = _context.Department
                 .Select(x => new SelectDepartment
                 {
                     DepartmentId = x.Id,
                     DepartmentName = x.Description
                 });
            var availableDepartments = _context.User
                .Where(x => x.FacilityId.Equals(facilityId) && x.Level.Equals(_roles.Value.DOCTOR))
                .GroupBy(d => d.DepartmentId)
                .Select(y => new SelectDepartment
                {
                    DepartmentId = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentId,
                    DepartmentName = departments.Single(x => x.DepartmentId.Equals(y.Key)).DepartmentName
                });
            return Task.FromResult(availableDepartments);
        }
        public PatientForm setNormalPatientForm(WalkinPatientViewModel model)
        {
            PatientForm patient = new PatientForm
            {
                UniqueId = model.PatientId + "-" + UserFacility() + "-" + DateTime.Now.ToString("yyMMddhh"),
                Code = DateTime.Now.ToString("yyMMdd") + "-" + UserFacility().ToString().PadLeft(3, '0') + "-" + DateTime.Now.ToString("hhmmss"),
                ReferringFacilityId = model.ReferringFacility,
                ReferredTo = UserFacility(),
                DepartmentId = model.Department,
                TimeReferred = DateTime.Now,
                TimeTransferred = default,
                PatientId = model.PatientId,
                CaseSummary = model.CaseSummary,
                RecommendSummary = model.SummaryReco,
                Diagnosis = model.Diagnosis,
                Reason = model.Reason,
                ReferringMd = null,
                ReferredMd = UserId(),
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return patient;
        }

        public PatientForm setNormalPatientForm(ReferPatientViewModel model)
        {
            PatientForm patient = new PatientForm
            {
                UniqueId = model.PatientId + "-" + UserFacility() + "-" + DateTime.Now.ToString("yyMMddhh"),
                Code = DateTime.Now.ToString("yyMMdd") + "-" + UserFacility().ToString().PadLeft(3, '0') + "-" + DateTime.Now.ToString("hhmmss"),
                ReferringFacilityId = UserFacility(),
                ReferredTo = model.ReferredTo,
                DepartmentId = model.Department,
                TimeReferred = DateTime.Now,
                TimeTransferred = default,
                PatientId = model.PatientId,
                CaseSummary = model.CaseSummary,
                RecommendSummary = model.SummaryReco,
                Diagnosis = model.Diagnosis,
                Reason = model.Reason,
                ReferringMd = UserId(),
                ReferredMd = model.ReferredToMd,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now                
            };
            return patient;
        }

        public Tracking setWalkinTracking(PatientForm patientForm)
        {
            Tracking tracking = new Tracking
            {
                Code = patientForm.Code,
                PatientId = patientForm.PatientId,
                DateReferred = patientForm.TimeReferred,
                DateTransferred = patientForm.TimeTransferred,
                DateAccepted = DateTime.Now,
                DateArrived = default,
                DateSeen = DateTime.Now,
                ReferredFrom = patientForm.ReferringFacilityId,
                ReferredTo = patientForm.ReferredTo,
                DepartmentId = patientForm.DepartmentId,
                ReferringMd = patientForm.ReferringMd,
                ActionMd = patientForm.ReferredMd,
                Remarks = patientForm.Reason,
                Type = "normal",
                Status = _status.Value.ACCEPTED,
                //FormId = patientForm.Id,
                WalkIn = "yes",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return tracking;
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

        private Activity SetReferredActivity(Tracking tracking)
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
                Remarks = tracking.Status,
                Status = _status.Value.REFERRED,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return activity;
        }

        public Activity SetWalkinActivity(Tracking tracking)
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
                ActionMd = UserId(),
                Remarks = "Walk-In Patient",
                Status = tracking.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return activity;
        }


        public Activity SetNormalActivity(Tracking tracking)
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
                ActionMd = UserId(),
                Remarks = tracking.Remarks,
                Status = tracking.Status,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            return activity;
        }


        public void setPatients(Patient patients)
        {
            patients.FirstName = patients.FirstName.Trim();
            patients.MiddleName = patients.MiddleName.Trim();
            patients.LastName = patients.LastName.Trim();
            patients.Address = GetAddress(patients);
            patients.PhicId = string.IsNullOrEmpty(patients.PhicId) ? "None" : patients.PhicId;
            patients.TsekapPatient = 0;
            patients.UniqueId = RemoveWhiteSpace(patients.FirstName + patients.MiddleName + patients.LastName) + RemoveDash(patients.DateOfBirth.ToString("yyyy/MM/dd")) + patients.BarangayId.ToString();
            patients.CreatedAt = DateTime.Now;
            patients.UpdatedAt = DateTime.Now;
            _context.Add(patients);
        }

        public string GetAddress(Patient patient)
        {
            string barangay = patient.Barangay == null ? "" : patient.Barangay.Description + ", ";
            string muncity = patient.Muncity == null ? "" : patient.Muncity.Description + ", ";
            string province = patient.Province == null ? "" : patient.Province.Description;

            return barangay + muncity + province;
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