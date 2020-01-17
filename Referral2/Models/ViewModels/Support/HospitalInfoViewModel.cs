using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Support
{
    public partial class HospitalInfoViewModel
    {
        public string FacilityName { get; set; }
        public string Abbreviation { get; set; }
        public int MuncityId { get; set; }
        public int BarangayId { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
    }
}
