using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public partial class DischargedViewModel
    {
        public string ReferringFacility { get; set; }
        public string Type { get; set; }
        public string PatientName { get; set; }
        public string Code { get; set; }
        public DateTime DateAction { get; set; }
        public string Status { get; set; }
    }
}
