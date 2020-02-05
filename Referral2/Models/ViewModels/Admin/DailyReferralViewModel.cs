
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class DailyReferralViewModel
    {
        public string Facility { get; set; }
        public int AcceptedTo { get; set; }
        public int RedirectedTo { get; set; }
        public int SeenTo { get; set; }
        public int UnseenTo { get; set; }
        public int AcceptedFrom { get; set; }
        public int RedirectedFrom { get; set; }
        public int SeenFrom { get; set; }
    }
}
