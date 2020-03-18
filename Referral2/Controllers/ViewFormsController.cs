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
using Referral2.Models.ViewModels.Forms;

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

            var patientForm = _context.PatientForm
                .Include(x => x.Patient).ThenInclude(x => x.Barangay)
                .Include(x => x.Patient).ThenInclude(x => x.Muncity)
                .Include(x=>x.Patient).ThenInclude(x=>x.Province)
                .Include(x=>x.ReferredToNavigation).ThenInclude(x=>x.Barangay)
                .Include(x => x.ReferredToNavigation).ThenInclude(x => x.Muncity)
                .Include(x => x.ReferredToNavigation).ThenInclude(x => x.Province)
                .Include(x=>x.ReferringFacility).ThenInclude(x=>x.Barangay)
                .Include(x => x.ReferringFacility).ThenInclude(x => x.Muncity)
                .Include(x => x.ReferringFacility).ThenInclude(x => x.Province)
                .Include(x=>x.Department)
                .Include(x=>x.ReferredMdNavigation)
                .Include(x=>x.ReferringMdNavigation)
                .Single(x => x.Code.Equals(code));

            if (patientForm == null)
                return NotFound();

            var tracking = _context.Tracking.Single(x => x.Code.Equals(code));
            var activity = _context.Activity.Single(x => x.Code.Equals(code) && x.Status.Equals(_status.Value.REFERRED));

            if (!activity.Status.Equals(_status.Value.REFERRED))
                activity.Status = _status.Value.REFERRED;

            tracking.DateSeen = DateTime.Now;
            tracking.Status = _status.Value.SEEN;
            tracking.UpdatedAt = DateTime.Now;
            activity.DateSeen = DateTime.Now;
            _context.Update(tracking);
            _context.Update(activity);

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
        public async Task<IActionResult> PregnantForm(string code)
        {
            var form = await _context.PregnantForm
                .Include(x=>x.PatientBaby)
                .Include(x=>x.PatientWoman)
                .Include(x=>x.Department)
                .Include(x=>x.ReferredToNavigation)
                .Include(x=>x.ReferredByNavigation)
                .Include(x=>x.ReferringFacilityNavigation)
                .SingleAsync(x => x.Code.Equals(code));
            Baby baby = null;

            if (form.PatientBabyId != null)
                baby = await _context
                    .Baby.SingleOrDefaultAsync(x => x.BabyId.Equals(form.PatientBabyId));

            var pregnantForm = new PregnantViewModel(form, baby);

            var tracking = _context.Tracking.Single(x => x.Code.Equals(code));
            var activity = _context.Activity.Single(x => x.Code.Equals(code) && x.Status.Equals(_status.Value.REFERRED));

            if (!activity.Status.Equals(_status.Value.REFERRED))
                activity.Status = _status.Value.REFERRED;

            tracking.DateSeen = DateTime.Now;
            tracking.Status = _status.Value.SEEN;
            activity.DateSeen = DateTime.Now;
            tracking.UpdatedAt = DateTime.Now;
            _context.Update(tracking);
            _context.Update(activity);

            var seen = new Seen
            {
                FacilityId = UserFacility(),
                TrackingId = _context.Tracking.Single(x => x.Code.Equals(form.Code)).Id,
                UpdatedAt = DateTime.Now,
                CreatedAt = DateTime.Now,
                UserMd = UserId()
            };

            await _context.AddAsync(seen);
            await _context.SaveChangesAsync();
            return PartialView(pregnantForm);
        }

        public async Task<IActionResult> PrintableNormalForm(string code)
        {
            var form = await _context.PatientForm
                .Include(x => x.Patient).ThenInclude(x => x.Barangay)
                .Include(x => x.Patient).ThenInclude(x => x.Muncity)
                .Include(x => x.Patient).ThenInclude(x => x.Province)
                .Include(x => x.ReferredToNavigation).ThenInclude(x => x.Barangay)
                .Include(x => x.ReferredToNavigation).ThenInclude(x => x.Muncity)
                .Include(x => x.ReferredToNavigation).ThenInclude(x => x.Province)
                .Include(x => x.ReferringFacility).ThenInclude(x => x.Barangay)
                .Include(x => x.ReferringFacility).ThenInclude(x => x.Muncity)
                .Include(x => x.ReferringFacility).ThenInclude(x => x.Province)
                .Include(x => x.Department)
                .Include(x => x.ReferredMdNavigation)
                .Include(x => x.ReferringMdNavigation)
                .SingleOrDefaultAsync(x => x.Code.Equals(code));

            return PartialView(form);
        }

        public async Task<IActionResult> PrintablePregnantForm(string code)
        {
            var form = _context.PregnantForm
                .Include(x => x.PatientBaby)
                .Include(x => x.PatientWoman).ThenInclude(x=>x.Barangay)
                .Include(x => x.PatientWoman).ThenInclude(x => x.Muncity)
                .Include(x => x.PatientWoman).ThenInclude(x => x.Province)
                .Include(x => x.Department)
                .Include(x => x.ReferredToNavigation).ThenInclude(x=>x.Barangay)
                .Include(x => x.ReferredToNavigation).ThenInclude(x => x.Muncity)
                .Include(x => x.ReferredToNavigation).ThenInclude(x => x.Province)
                .Include(x => x.ReferredByNavigation)
                .Include(x => x.ReferringFacilityNavigation).FirstOrDefault(x => x.Code.Equals(code));

            Baby baby = null;

            if(form.PatientBabyId != null)
                baby = await _context.Baby.SingleOrDefaultAsync(x => x.BabyId.Equals(form.PatientBabyId));

            var pregnantForm = new PregnantViewModel(form, baby);

            return PartialView(pregnantForm);
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