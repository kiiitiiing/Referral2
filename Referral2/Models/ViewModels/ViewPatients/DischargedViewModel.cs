using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public class DischargedViewModel
    {
        public string Code { get; set; }
        public Patient Patient { get; set; }
        public string Status { get; set; }
        public DateTime DateAction { get; set; }
        public Facility ReferredFrom { get; set; }
        public string Type { get; set; }
    }
}
