using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Support
{
    public partial class IncomingReferralViewModel
    {
        public string PatientName { get; set; }
        public string ReferringFacility { get; set; }
        public string Department { get; set; }
        public DateTime DateReferred { get; set; }
        public string Status { get; set; }
    }
}
