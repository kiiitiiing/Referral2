using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public partial class ReferredViewModel
    {
        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public int PatientAge { get; set; } 
        public int TrackingId { get; set; }
        public int SeenCount { get; set; }
        public int CallerCount { get; set; }
        public bool Travel { get; set; }
        public int ReCoCount { get; set; }
        public string PatientAddress { get; set; }
        public string ReferredBy { get; set; }
        public string ReferredTo { get; set; }
        public bool Pregnant { get; set; }
        public string Code { get; set; }
        public string Status { get; set; }
        public bool Walkin { get; set; }
        public IEnumerable<Activity> Activities { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
