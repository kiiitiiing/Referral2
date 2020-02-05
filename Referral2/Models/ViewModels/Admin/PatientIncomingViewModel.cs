using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class PatientIncomingViewModel
    {
        public string DateReferred { get; set; }
        public string DateArrived { get; set; }
        public string PatientName { get; set; }
        public string PatientAge { get; set; }
        public string PatientSex { get; set; }
        public string PatientAddress { get; set; }
        public string ReferredFrom { get; set; }
        public string Diagnosis { get; set; }
        public string ReferringMd { get; set; }
        public string Reason { get; set; }
        public string ReferredMd { get; set; }
        public string Transportation { get; set; }
        public string Reco { get; set; }
        public string Acknowledgements { get; set; }
        public string Remarks { get; set; }
    }
}
