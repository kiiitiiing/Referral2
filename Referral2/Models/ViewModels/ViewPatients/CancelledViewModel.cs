using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public partial class CancelledViewModel
    {
        public string ReferringFacility { get; set; }
        public string PatientType { get; set; }
        public string PatientName { get; set; }
        public string PatientCode { get; set; }
        public DateTime DateCancelled { get; set; }
        public string ReasonCancelled { get; set; }
    }
}
