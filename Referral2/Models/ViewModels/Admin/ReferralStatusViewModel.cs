using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class ReferralStatusViewModel
    {
        public DateTime DateReferred { get; set; }
        public string FacilityFrom { get; set; }
        public string FacilityTo { get; set; }
        public string Department { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; }
    }
}
