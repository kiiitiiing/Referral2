using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Referral2.Data;
using Referral2.Helpers;

namespace Referral2.Controllers
{
    public class BaseController : Controller
    {
        private readonly ReferralDbContext _context;
        private readonly IOptions<ReferralRoles> _roles;
        private readonly IOptions<ReferralStatus> _status;


        public BaseController( ReferralDbContext context, IOptions<ReferralRoles> roles, IOptions<ReferralStatus> status)
        {
            _context = context;
            _roles = roles;
            _status = status;
        }

        public BaseController()
        { }
    }
}