using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.Modals
{
    public class SearchModel
    {
        public string Action { get; set; }
        public string Search { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
