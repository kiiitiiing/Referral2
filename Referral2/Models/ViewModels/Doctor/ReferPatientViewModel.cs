using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class ReferPatientViewModel
    {
        public string ReferringFacility { get; set; }
        public string ReferringFacilityAddress { get; set; }
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientSex { get; set; }
        public string PatientCivilStatus { get; set; }
        public string PatientAddress { get; set; }
        public string PatientPhicStatus { get; set; }
        public string PatientPhicId { get; set; }

        //Send form
        public int ReferredTo { get; set; }
        public int Department { get; set; }
        public string CaseSummary { get; set; }
        public string SummaryReco { get; set; }
        public string Diagnosis { get; set; }
        public string Reason { get; set; }
        public int ReferredToMd { get; set; }
    }
}
