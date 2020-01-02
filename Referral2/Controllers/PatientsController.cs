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
    public class PatientsController : Controller
    {
        private readonly ReferralDbContext _context;

        public PatientsController(ReferralDbContext context)
        {
            _context = context;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            var referralDbContext = _context.Patient.Include(p => p.Barangay).Include(p => p.Muncity).Include(p => p.Province);
            return View(await referralDbContext.ToListAsync());
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient
                .Include(p => p.Barangay)
                .Include(p => p.Muncity)
                .Include(p => p.Province)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            ViewData["BarangayId"] = new SelectList(_context.Barangay, "Id", "Description");
            ViewData["MuncityId"] = new SelectList(_context.Muncity, "Id", "Description");
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "Description");
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UniqueId,FirstName,MiddleName,LastName,DateOfBirth,Sex,CivilStatus,PhicId,PhicStatus,Address,BarangayId,MuncityId,ProvinceId,TsekapPatient,CreatedAt,UpdatedAt")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BarangayId"] = new SelectList(_context.Barangay, "Id", "Description", patient.BarangayId);
            ViewData["MuncityId"] = new SelectList(_context.Muncity, "Id", "Description", patient.MuncityId);
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "Description", patient.ProvinceId);
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            ViewData["BarangayId"] = new SelectList(_context.Barangay, "Id", "Description", patient.BarangayId);
            ViewData["MuncityId"] = new SelectList(_context.Muncity, "Id", "Description", patient.MuncityId);
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "Description", patient.ProvinceId);
            return View(patient);
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UniqueId,FirstName,MiddleName,LastName,DateOfBirth,Sex,CivilStatus,PhicId,PhicStatus,Address,BarangayId,MuncityId,ProvinceId,TsekapPatient,CreatedAt,UpdatedAt")] Patient patient)
        {
            if (id != patient.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
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
            ViewData["BarangayId"] = new SelectList(_context.Barangay, "Id", "Description", patient.BarangayId);
            ViewData["MuncityId"] = new SelectList(_context.Muncity, "Id", "Description", patient.MuncityId);
            ViewData["ProvinceId"] = new SelectList(_context.Province, "Id", "Description", patient.ProvinceId);
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Patient
                .Include(p => p.Barangay)
                .Include(p => p.Muncity)
                .Include(p => p.Province)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patient.FindAsync(id);
            _context.Patient.Remove(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patient.Any(e => e.Id == id);
        }
    }
}
