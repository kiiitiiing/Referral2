using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Helpers
{
    public partial class ReferralStatus
    {
        public string SEEN {get;set;}
        public string TRAVEL { get; set; }
        public string ARRIVED {get;set;}
        public string CALLING {get;set;}
        public string ACCEPTED {get;set;}
        public string ADMITTED {get;set;}
        public string ARCHIVED {get;set; }
        public string REDIRECTED { get; set; }
        public string REFERRED {get;set;}
        public string REJECTED {get;set;}
        public string CANCELLED {get;set;}
        public string DISCHARGED {get;set;}
        public string TRANSFERRED {get;set;}
    }
}
