using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;

namespace Referral2.Controllers
{
    public class ViewFormsController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public ViewFormsController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        public async Task<IActionResult> PatientForm(string code)
        {
            if (code == null)
                return NotFound();

            var patientForm = _context.PatientForm.Single(x => x.Code.Equals(code));

            /*var patientForm = from s in _context.PatientForm
                              select new PatientFormViewModel
                              {
                                  ReferringFacility = s.ReferringFacility.Name,
                                  ReferringFacilityAddress = s.ReferringFacility.Address,
                                  ReferringFacilityBarangay = s.ReferringFacility.Barangay == null ? "" : s.ReferringFacility.Barangay.Description,
                                  ReferringFacilityMuncity = s.ReferringFacility.Muncity == null ? "" : s.ReferringFacility.Muncity.Description,
                                  ReferringFacilityProvince = s.ReferringFacility.Province == null ? "": s.ReferringFacility.Province.Description,
                                  ReferredToFacility = s.ReferredToNavigation.Name,
                                  ReferredToDepartment = s.Department.Description,
                                  ReferredToAddress = s.ReferredToNavigation.Address,
                                  ReferredToBarangay = s.ReferredToNavigation.Barangay == null? "": s.ReferredToNavigation.Barangay.Description,
                                  ReferredToMuncity = s.ReferredToNavigation.Muncity == null? "" : s.ReferredToNavigation.Muncity.Description,
                                  ReferredToProvince = s.ReferredToNavigation.Province == null? "" : s.ReferredToNavigation.Province.Description,
                                  DateReferred = s.TimeReferred,
                                  DateTransferred = s.TimeTransferred,
                                  PatientName = s.Patient.FirstName+" "+s.Patient.MiddleName+" "+s.Patient.LastName,
                                  PatientAge = GlobalFunctions.ComputeAge(s.Patient.DateOfBirth),
                                  PatientSex = s.Patient.Sex,
                                  PatientCivilStatus = s.Patient.CivilStatus,
                                  PatientAddress = s.Patient.Barangay.Description+", "+s.Patient.Muncity.Description+", "+s.Patient.Province.Description,
                                  PatientPhicStatus = s.Patient.PhicStatus,
                                  PatientPhicId = s.Patient.PhicId,
                                  CaseSummary = 

                              }*/

            if (patientForm == null)
                return NotFound();

            var tracking = _context.Tracking.Single(x => x.Code.Equals(code));
            var activity = _context.Activity.Single(x => x.Code.Equals(code) && x.Status.Equals(_status.Value.REFERRED));

            if (!activity.Status.Equals(_status.Value.REFERRED))
                activity.Status = _status.Value.REFERRED;

            tracking.DateSeen = DateTime.Now;
            _context.Update(tracking);

            var seen = new Seen();
            seen.FacilityId = UserFacility();
            seen.TrackingId = _context.Tracking.Single(x => x.Code.Equals(patientForm.Code)).Id;
            seen.UpdatedAt = DateTime.Now;
            seen.CreatedAt = DateTime.Now;
            seen.UserMd = UserId();
            _context.Add(seen);
            await _context.SaveChangesAsync();
            return PartialView(patientForm);
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