using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Referral2.Data;
using Referral2.Models;

namespace Referral2.Controllers
{
    public class PatientFormsController : Controller
    {
        private readonly ReferralDbContext _context;

        public PatientFormsController(ReferralDbContext context)
        {
            _context = context;
        }

        // GET: PatientForms
        public async Task<IActionResult> Index()
        {
            var referralDbContext = _context.PatientForm.Include(p => p.Department).Include(p => p.Patient).Include(p => p.ReferredMdNavigation).Include(p => p.ReferredToNavigation).Include(p => p.ReferringFacility).Include(p => p.ReferringMdNavigation);
            return View(await referralDbContext.ToListAsync());
        }

        // GET: PatientForms/Details/5
        public async Task<IActionResult> Details(string code)
        {
            if (code == null)
            {
                return NotFound();
            }

            var patientForm = await _context.PatientForm
                .Include(p => p.Department)
                .Include(p => p.Patient)
                .Include(p => p.ReferredMdNavigation)
                .Include(p => p.ReferredToNavigation)
                .Include(p => p.ReferringFacility)
                .Include(p => p.ReferringMdNavigation)
                .FirstOrDefaultAsync(m => m.Code == code);
            if (patientForm == null)
            {
                return NotFound();
            }

            return View(patientForm);
        }

        // GET: PatientForms/Create
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description");
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus");
            ViewData["ReferredMd"] = new SelectList(_context.User, "Id", "Contact");
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id");
            ViewData["ReferringFacilityId"] = new SelectList(_context.Facility, "Id", "Id");
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact");
            return View();
        }

        // POST: PatientForms/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UniqueId,Code,ReferringFacilityId,ReferredTo,DepartmentId,TimeReferred,TimeTransferred,PatientId,CaseSummary,RecommendSummary,Diagnosis,Reason,ReferringMd,ReferredMd,CreatedAt,UpdatedAt")] PatientForm patientForm)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patientForm);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description", patientForm.DepartmentId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus", patientForm.PatientId);
            ViewData["ReferredMd"] = new SelectList(_context.User, "Id", "Contact", patientForm.ReferredMd);
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id", patientForm.ReferredTo);
            ViewData["ReferringFacilityId"] = new SelectList(_context.Facility, "Id", "Id", patientForm.ReferringFacilityId);
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact", patientForm.ReferringMd);
            return View(patientForm);
        }

        // GET: PatientForms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientForm = await _context.PatientForm.FindAsync(id);
            if (patientForm == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description", patientForm.DepartmentId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus", patientForm.PatientId);
            ViewData["ReferredMd"] = new SelectList(_context.User, "Id", "Contact", patientForm.ReferredMd);
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id", patientForm.ReferredTo);
            ViewData["ReferringFacilityId"] = new SelectList(_context.Facility, "Id", "Id", patientForm.ReferringFacilityId);
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact", patientForm.ReferringMd);
            return View(patientForm);
        }

        // POST: PatientForms/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UniqueId,Code,ReferringFacilityId,ReferredTo,DepartmentId,TimeReferred,TimeTransferred,PatientId,CaseSummary,RecommendSummary,Diagnosis,Reason,ReferringMd,ReferredMd,CreatedAt,UpdatedAt")] PatientForm patientForm)
        {
            if (id != patientForm.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientForm);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientFormExists(patientForm.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description", patientForm.DepartmentId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus", patientForm.PatientId);
            ViewData["ReferredMd"] = new SelectList(_context.User, "Id", "Contact", patientForm.ReferredMd);
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id", patientForm.ReferredTo);
            ViewData["ReferringFacilityId"] = new SelectList(_context.Facility, "Id", "Id", patientForm.ReferringFacilityId);
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact", patientForm.ReferringMd);
            return View(patientForm);
        }

        // GET: PatientForms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientForm = await _context.PatientForm
                .Include(p => p.Department)
                .Include(p => p.Patient)
                .Include(p => p.ReferredMdNavigation)
                .Include(p => p.ReferredToNavigation)
                .Include(p => p.ReferringFacility)
                .Include(p => p.ReferringMdNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patientForm == null)
            {
                return NotFound();
            }

            return View(patientForm);
        }

        // POST: PatientForms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patientForm = await _context.PatientForm.FindAsync(id);
            _context.PatientForm.Remove(patientForm);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientFormExists(int id)
        {
            return _context.PatientForm.Any(e => e.Id == id);
        }
    }
}
