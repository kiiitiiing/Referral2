using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;

namespace Referral2.Controllers
{
    public class ViewFormsController : Controller
    {
        private readonly ReferralDbContext _context;

        public ViewFormsController(ReferralDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PatientForm(string code)
        {
            if (code == null)
                return NotFound();

            var patientForm = _context.PatientForm
                                        .Include(x=>x.Patient)
                                        .Include(x=>x.ReferringFacility)
                                        .Include(x=>x.ReferredToNavigation)
                                        .Include(x=>x.ReferringMdNavigation)
                                        .Include(x=>x.ReferredMdNavigation)
                                        .Include(x=>x.Department)
                                        .FirstOrDefault(x => x.Code == code);

            if (patientForm == null)
                return NotFound();

            var seen = new Seen();
            seen.FacilityId = CurrentUser.user.FacilityId;
            seen.TrackingId = _context.Tracking.Single(x => x.Code.Equals(patientForm.Code)).Id;
            seen.UpdatedAt = DateTime.Now;
            seen.CreatedAt = DateTime.Now;
            seen.UserMd = CurrentUser.user.Id;
            _context.Add(seen);
            await _context.SaveChangesAsync();
            return PartialView(patientForm);
        }
    }
}