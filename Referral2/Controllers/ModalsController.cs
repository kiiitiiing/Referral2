using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;
using Referral2.Models;
using Referral2.Models.ViewModels;

namespace Referral2.Controllers
{
    public class ModalsController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;

        public ModalsController(ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        
        // Get Seens
        public async Task<IActionResult> ViewSeens(int trackingId)
        {
            var seens = _context.Seen
                .Where(x => x.TrackingId.Equals(trackingId))
                .Select(x => new SeenCallViewModel
                {
                    MdName = GlobalFunctions.GetMDFullName(x.UserMdNavigation),
                    SeenDate = (DateTime)x.CreatedAt,
                    MdContact = x.UserMdNavigation.Contact
                });

            return PartialView(await seens.AsNoTracking().ToListAsync());
        }

        // Get calls
        public async Task<IActionResult> ViewCalls(string code)
        {
            var calls = _context.Activity
                .Where(x => x.Code.Equals(code) && x.Status.Equals(_status.Value.CALLING))
                .Select(x => new SeenCallViewModel
                {
                    MdName = GlobalFunctions.GetMDFullName(x.ActionMdNavigation),
                    SeenDate = (DateTime)x.DateReferred,
                    MdContact = x.ActionMdNavigation.Contact
                });

            return PartialView(await calls.AsNoTracking().ToListAsync());
        }

        // Get Feedback
        public IActionResult ViewReco(string code)
        {
            var feedbacks = _context.Feedback
                .Include(x=>x.Sender).ThenInclude(x=>x.Facility)
                .Where(x => x.Code.Equals(code)).ToList();

            var chats = new ChatsModel
            {
                Chats = feedbacks,
                Code = code
            };

            return PartialView("~/Views/Modals/ViewReco.cshtml", chats);
        }

        // POST Feedback
        [HttpPost]
        public async Task<IActionResult> ViewReco([Bind] ChatsModel model)
        {
            if (ModelState.IsValid)
            {
                var feedback = new Feedback
                {
                    Code = model.Code,
                    SenderId = model.Sender,
                    RecieverId = null,
                    Message = model.Message,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                  _context.Add(feedback);
                  await _context.SaveChangesAsync();
                ViewBag.IsValid = true;
            }
            var chatsModel = new ChatsModel()
            {
                Chats = _context.Feedback
                .Include(x => x.Sender).ThenInclude(x => x.Facility).Where(x => x.Code == model.Code).ToList(),
                Code = model.Code
            };
            ViewBag.IsValid = false;
            return PartialView(chatsModel);
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