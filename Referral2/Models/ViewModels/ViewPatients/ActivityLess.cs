using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public partial class ActivityLess
    {
        public string Remarks { get; set; }
        public string DateAction { get; set; }
        public string PatientName { get; set; }
        public string FacilityFrom { get; set; }
        public string FacilityFromContact { get; set; }
        public string FacilityTo { get; set; }
        public string ActionMd { get; set; }
        public string ActionMdFacility { get; set; }
        public string ReferringMd { get; set; }
        public string Status { get; set; }
    }
}
