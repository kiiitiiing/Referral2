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
    public class TrackingsController : Controller
    {
        private readonly ReferralDbContext _context;

        public TrackingsController(ReferralDbContext context)
        {
            _context = context;
        }

        // GET: Trackings
        public async Task<IActionResult> Index()
        {
            var referralDbContext = _context.Tracking.Include(t => t.ActionMdNavigation).Include(t => t.Department).Include(t => t.Patient).Include(t => t.ReferredFromNavigation).Include(t => t.ReferredToNavigation).Include(t => t.ReferringMdNavigation);
            
            return View(await referralDbContext.ToListAsync());
        }

        // GET: Trackings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tracking = await _context.Tracking
                .FirstOrDefaultAsync(m => m.Id == id);

            ViewBag.Seens = _context.Seen.Where(x => x.TrackingId.Equals(tracking.Id)).Count();
            if (tracking == null)
            {
                return NotFound();
            }

            return View(tracking);
        }

        // GET: Trackings/Create
        public IActionResult Create()
        {
            ViewData["ActionMd"] = new SelectList(_context.User, "Id", "Contact");
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description");
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus");
            ViewData["ReferredFrom"] = new SelectList(_context.Facility, "Id", "Id");
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id");
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact");
            return View();
        }

        // POST: Trackings/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Code,PatientId,DateReferred,DateTransferred,DateAccepted,DateArrived,DateSeen,Transportation,ReferredFrom,ReferredTo,DepartmentId,ReferringMd,ActionMd,Remarks,Status,Type,WalkIn,FormId,CreatedAt,UpdatedAt")] Tracking tracking)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tracking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ActionMd"] = new SelectList(_context.User, "Id", "Contact", tracking.ActionMd);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description", tracking.DepartmentId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus", tracking.PatientId);
            ViewData["ReferredFrom"] = new SelectList(_context.Facility, "Id", "Id", tracking.ReferredFrom);
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id", tracking.ReferredTo);
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact", tracking.ReferringMd);
            return View(tracking);
        }

        // GET: Trackings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tracking = await _context.Tracking.FindAsync(id);
            if (tracking == null)
            {
                return NotFound();
            }
            ViewData["ActionMd"] = new SelectList(_context.User, "Id", "Contact", tracking.ActionMd);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description", tracking.DepartmentId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus", tracking.PatientId);
            ViewData["ReferredFrom"] = new SelectList(_context.Facility, "Id", "Id", tracking.ReferredFrom);
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id", tracking.ReferredTo);
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact", tracking.ReferringMd);
            return View(tracking);
        }

        // POST: Trackings/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Code,PatientId,DateReferred,DateTransferred,DateAccepted,DateArrived,DateSeen,Transportation,ReferredFrom,ReferredTo,DepartmentId,ReferringMd,ActionMd,Remarks,Status,Type,WalkIn,FormId,CreatedAt,UpdatedAt")] Tracking tracking)
        {
            if (id != tracking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tracking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrackingExists(tracking.Id))
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
            ViewData["ActionMd"] = new SelectList(_context.User, "Id", "Contact", tracking.ActionMd);
            ViewData["DepartmentId"] = new SelectList(_context.Department, "Id", "Description", tracking.DepartmentId);
            ViewData["PatientId"] = new SelectList(_context.Patient, "Id", "CivilStatus", tracking.PatientId);
            ViewData["ReferredFrom"] = new SelectList(_context.Facility, "Id", "Id", tracking.ReferredFrom);
            ViewData["ReferredTo"] = new SelectList(_context.Facility, "Id", "Id", tracking.ReferredTo);
            ViewData["ReferringMd"] = new SelectList(_context.User, "Id", "Contact", tracking.ReferringMd);
            return View(tracking);
        }

        // GET: Trackings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tracking = await _context.Tracking
                .Include(t => t.ActionMdNavigation)
                .Include(t => t.Department)
                .Include(t => t.Patient)
                .Include(t => t.ReferredFromNavigation)
                .Include(t => t.ReferredToNavigation)
                .Include(t => t.ReferringMdNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tracking == null)
            {
                return NotFound();
            }

            return View(tracking);
        }

        // POST: Trackings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tracking = await _context.Tracking.FindAsync(id);
            _context.Tracking.Remove(tracking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrackingExists(int id)
        {
            return _context.Tracking.Any(e => e.Id == id);
        }
    }
}
