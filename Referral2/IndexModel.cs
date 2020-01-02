using Microsoft.AspNetCore.Mvc.RazorPages;
using Referral2.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2
{
    public class IndexModel : PageModel
    {
        private readonly ReferralDbContext _context;

        public IndexModel(ReferralDbContext context)
        {
            _context = context;
        }

        public string FirstNameSort { get; set; }
        public string MiddelNameSort { get; set; }
        public string LastNameSort { get; set; }
        public string CodeSort { get; set; }
    }
}
