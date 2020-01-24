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
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;
using Referral2.Resources;
using Microsoft.Extensions.Options;
using Referral2.Models.ViewModels.Remarks;

namespace Referral2.Controllers
{
    [Authorize]
    public class RemarksController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;


        public RemarksController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        public string Code { get; set; }

        public int Id { get; set; }

        #region ISSUES AND CONCERN

        public IActionResult Issues(int? id)
        {

            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> Issues([Bind]Issue model)
        {
            return PartialView();
        }

        #endregion

        #region TRAVEL

        public IActionResult Travel(int? id)
        {
            var transportations = _context.Transportation;
            ViewBag.TrackingId = id;
            ViewBag.Transpo = new SelectList(transportations, "Id", "Transportation1");
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> Travel([Bind]TravelViewModel model)
        {
            if(ModelState.IsValid)
            {
                var tracking = TravelTracking(model);
                var activity = TravelActivity(tracking);
                _context.Update(tracking);
                _context.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction("Referred", "ViewPatients");
            }
            return PartialView();
        }

        

        #endregion

        #region ACCEPTED
        [HttpGet]
        public IActionResult AcceptedRemark(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AcceptedRemark([Bind] RemarksViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = AcceptedTracking(model, _status.Value.ACCEPTED);
                var activity = NewActivity(tracking, DateTime.Now);
                _context.Update(tracking);
                _context.Add(activity);
                await _context.SaveChangesAsync();

                return RedirectToAction("Incoming", "ViewPatients");
            }
            return PartialView(model);
        }
        #endregion

        #region ARRIVED
        [HttpGet]
        public IActionResult ArrivedRemark(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> ArrivedRemark([Bind] RemarksViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = ArrivedTracking(model, _status.Value.ARRIVED);
                var activity = NewActivity(tracking, DateTime.Now);
                _context.Update(tracking);
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Accepted", "ViewPatients");
            }
            return PartialView(model);
        }
        #endregion

        #region DIDNT ARRIVE
        [HttpGet]
        public IActionResult DidntArrivedRemark(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> DidntArrivedRemark([Bind] RemarksViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = ArrivedTracking(model, _status.Value.ARCHIVED);
                var activity = NewActivity(tracking, DateTime.Now);
                _context.Update(tracking);
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Accepted", "ViewPatients");
            }
            return PartialView(model);
        }
        #endregion

        #region ADMITTED
        [HttpGet]
        public IActionResult AdmittedRemark(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> AdmittedRemark([Bind] AdmittedViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = AdmittedTracking(model, _status.Value.ADMITTED);
                var activity = NewActivity(tracking, model.DateAdmitted);

                _context.Update(tracking);
                _context.Add(activity);

                await _context.SaveChangesAsync();
                return RedirectToAction("Accepted", "ViewPatients");
            }
            return PartialView(model);
        }
        #endregion

        #region CALLED
        [HttpGet]
        public IActionResult CallRequest(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> CallRequest([Bind] RemarksViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));
                NewActivity(tracking, DateTime.Now);
                await _context.SaveChangesAsync();
                return RedirectToAction("Incoming", "ViewPatients");
            }

            return PartialView(model);
        }
        #endregion

        #region REJECTED
        [HttpGet]
        public IActionResult RejectRemarks(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> RejectRemarks([Bind] RemarksViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = RejectedTracking(model, _status.Value.REJECTED);
                var activity = NewActivity(tracking, DateTime.Now);
                _context.Update(tracking);
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Incoming", "ViewPatients");
            }
            return PartialView(model);
        }
        #endregion

        #region REFER
        [HttpGet]
        public IActionResult ReferRemark(string code)
        {
            ViewBag.Facility = new SelectList(_context.Facility,"Id","Name");
            ViewBag.Department = new SelectList(_context.Department, "Id", "Description");
            return PartialView();
        }
        [HttpPost]
        public async Task<IActionResult> ReferRemark([Bind] ReferViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = ReferTracking(model, _status.Value.TRANSFERRED);
                var activity = NewActivity(tracking, DateTime.Now);
                var newTracking = NewTracking(tracking, model);
                
                _context.Update(tracking);
                _context.Add(newTracking);
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Accepted", "ViewPatients");
            }
            ViewBag.Facility = new SelectList(_context.Facility, "Id", "Name", model.FacilityId);
            ViewBag.Department = new SelectList(_context.Department, "Id", "Description", model.DepartmentId);
            return PartialView(model);
        }
        #endregion

        #region CANCEL

        [HttpGet]
        public IActionResult CancelRemark(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> CancelRemark([Bind]RemarksViewModel model)
        {
            if(ModelState.IsValid)
            {
                var tracking = CancelTracking(model, _status.Value.CANCELLED);
                var activity = NewActivity(tracking, DateTime.Now);
                _context.Update(activity);
                _context.Update(tracking);
                await _context.SaveChangesAsync();
                return RedirectToAction("Referred", "ViewPatients");
            }
            return PartialView();
        }

        #endregion

        #region DISCHARGED
        [HttpGet]
        public IActionResult DischargedRemark(string code)
        {
            return PartialView();
        }

        [HttpPost]
        public async Task<IActionResult> DischargedRemark([Bind] DischargeRemarkViewModel model, string code)
        {
            if (ModelState.IsValid)
            {
                var tracking = DischargedTracking(model, _status.Value.DISCHARGED);
                _context.Update(tracking);
                var activity = NewActivity(tracking, model.DateDischarged);
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Accepted", "ViewPatients");
            }
            return PartialView(model);
        }
        #endregion

        #region RECO
        [HttpGet]
        public IActionResult Recommend(string code)
        {
            var feedback = _context.Feedback.Where(c => c.Code.Equals(code));

            return PartialView(feedback.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Recommend([Bind] Feedback model, string code)
        {
            var currentTracking = _context.Tracking.FirstOrDefault(c => c.Code.Equals(code));
            if (model != null)
            {
                var newFeedback = new Feedback();
                newFeedback.Code = code;
                newFeedback.SenderId = UserId();;
                newFeedback.RecieverId = currentTracking.ReferringMd;
                newFeedback.Message = model.Message;
                newFeedback.CreatedAt = DateTime.Now;
                newFeedback.UpdatedAt = DateTime.Now;

                _context.Add(newFeedback);
                await _context.SaveChangesAsync();

                return RedirectToAction("Recommend", "Remarks");
            }
            return PartialView(model);
        }
        #endregion

        #region Helpers


        private Tracking TravelTracking(TravelViewModel model)
        {
            var tracking = _context.Tracking.Find(model.TrackingId);
            tracking.Transportation = model.TranspoId.ToString();
            tracking.UpdatedAt = DateTime.Now;

            return tracking;
        }



        private Tracking NewTracking(Tracking tracking, ReferViewModel model)
        {
            var newTracking = new Tracking();

            newTracking.Code = tracking.Code;   
            newTracking.PatientId = tracking.PatientId;
            newTracking.Transportation = null;
            newTracking.ReferredFrom = 
            newTracking.ReferredTo = model.FacilityId;
            newTracking.DepartmentId = model.DepartmentId;
            newTracking.ReferringMd = UserId();;
            newTracking.ActionMd = null;
            newTracking.DateReferred = DateTime.Now;
            newTracking.DateAccepted = default;
            newTracking.DateArrived = default;
            newTracking.DateSeen = default;
            newTracking.DateTransferred = default;
            newTracking.Remarks = model.Remarks;
            newTracking.Status = _status.Value.REFERRED;
            newTracking.Type = tracking.Type;
            newTracking.WalkIn = tracking.WalkIn;
            newTracking.FormId = tracking.FormId;
            newTracking.CreatedAt = DateTime.Now;
            newTracking.UpdatedAt = DateTime.Now;

            return newTracking;
        }

        private Tracking CancelTracking(RemarksViewModel model, string status)
        {
            var currentTracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));
            currentTracking.Status = status;
            currentTracking.Remarks = model.Remarks;
            currentTracking.UpdatedAt = DateTime.Now;
            return currentTracking;
        }

        private Tracking RejectedTracking(RemarksViewModel model, string status)
        {
            var currentTracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));

            return currentTracking;
        }

        private Tracking ArrivedTracking(RemarksViewModel model, string status)
        {
            var currentTracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));
            currentTracking.Remarks = model.Remarks;
            currentTracking.Status = status;
            currentTracking.DateArrived = DateTime.Now;
            currentTracking.UpdatedAt = DateTime.Now;

            return currentTracking;
        }

        public Tracking DischargedTracking(DischargeRemarkViewModel model, string status)
        {
            var currentTracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));
            currentTracking.Remarks = model.Remarks;
            currentTracking.Status = status;
            currentTracking.UpdatedAt = DateTime.Now;

            return currentTracking;
        }

        public Tracking ReferTracking(ReferViewModel model, string status)
        {
            var currentTracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));
            currentTracking.Remarks = model.Remarks;
            currentTracking.ActionMd = UserId();;
            currentTracking.Status = status;
            currentTracking.DateTransferred = DateTime.Now;
            currentTracking.UpdatedAt = DateTime.Now;

            return currentTracking;
        }


        public Tracking AdmittedTracking(AdmittedViewModel model, string status)
        {
            var currentTracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));
            currentTracking.Remarks =_status.Value.ADMITTED;
            currentTracking.Status = status;
            currentTracking.UpdatedAt = DateTime.Now;

            return currentTracking;
        }

        public Tracking AcceptedTracking(RemarksViewModel model , string status)
        {
            var updateTracking = _context.Tracking.FirstOrDefault(x => x.Code.Equals(model.Code));
            updateTracking.Remarks = model.Remarks;
            updateTracking.Status = status;
            updateTracking.DateAccepted = DateTime.Now;
            updateTracking.UpdatedAt = DateTime.Now;

            return updateTracking;
        }

        private Activity TravelActivity(Tracking tracking)
        {
            Activity activity = new Activity();
            activity.Code = tracking.Code;
            activity.PatientId = tracking.PatientId;
            activity.DateReferred = DateTime.Now;
            activity.DateSeen = default;
            activity.CreatedAt = DateTime.Now;
            activity.UpdatedAt = DateTime.Now;
            activity.ReferredFrom = UserFacility();
            activity.ReferredTo = UserFacility();
            activity.DepartmentId = UserDepartment();
            activity.ReferringMd = UserId();
            activity.ActionMd = UserId();
            activity.Remarks = tracking.Transportation;
            activity.Status = _status.Value.TRAVEL;
            return activity;
        }



        private Activity NewActivity(Tracking tracking, DateTime dateAction)
        {
            Activity activity = new Activity();

            activity.Code = tracking.Code;
            activity.PatientId = tracking.PatientId;
            activity.DateReferred = dateAction;
            activity.DateSeen = default;
            activity.CreatedAt = DateTime.Now;
            activity.UpdatedAt = DateTime.Now;

            switch (tracking.Status)
            {
                case "referred":
                    {
                        break;
                    }
                case "seen":
                    {
                        break;
                    }
                case "admitted":
                    {
                        activity.Remarks = tracking.Remarks;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredTo;
                        activity.ReferredTo = null;
                        activity.DepartmentId = tracking.DepartmentId;
                        activity.ReferringMd = null;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
                case "accepted":
                    {
                        activity.Remarks = tracking.Remarks;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredFrom;
                        activity.ReferredTo = tracking.ReferredTo;
                        activity.DepartmentId = tracking.DepartmentId;
                        activity.ReferringMd = tracking.ReferringMd;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
                case "arrived":
                    {
                        activity.Remarks = tracking.Remarks;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredTo;
                        activity.ReferredTo = null;
                        activity.DepartmentId = tracking.DepartmentId;
                        activity.ReferringMd = null;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
                case "calling":
                    {
                        activity.Remarks = UserName()+" called " + tracking.ReferredFromNavigation.Name;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredFrom;
                        activity.ReferredTo = tracking.ReferredTo;
                        activity.DepartmentId = tracking.DepartmentId;
                        activity.ReferringMd = null;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
                case "discharged":
                    {
                        activity.Remarks = tracking.Remarks;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredTo;
                        activity.ReferredTo = null;
                        activity.DepartmentId = null;
                        activity.ReferringMd = null;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
                case "transferred":
                    {
                        activity.Remarks = tracking.Remarks;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredTo;
                        activity.ReferredTo = tracking.ReferredFrom;
                        activity.DepartmentId = null;
                        activity.ReferringMd = null;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
                case "cancelled":
                    {
                        activity.Remarks = tracking.Remarks;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredFrom;
                        activity.ReferredTo = null;
                        activity.DepartmentId = null;
                        activity.ReferringMd = null;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
                case "rejected":
                    {
                        activity.Remarks = UserName() + " called " + tracking.ReferredFromNavigation.Name;
                        activity.DateSeen = default;
                        activity.ReferredFrom = tracking.ReferredFrom;
                        activity.ReferredTo = tracking.ReferredTo;
                        activity.DepartmentId = null;
                        activity.ReferringMd = null;
                        activity.ActionMd = UserId();;
                        activity.Status = tracking.Status;
                        break;
                    }
            }
            return activity;
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