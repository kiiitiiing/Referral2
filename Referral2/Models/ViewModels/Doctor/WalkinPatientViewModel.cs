using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class WalkinPatientViewModel : PatientViewModel
    {
        public string ReferredTo { get; set; }
        public string ReferredToAddress { get; set; }

        //Send form
        public int ReferringFacility { get; set; }
        public int Department { get; set; }
        public string CaseSummary { get; set; }
        public string SummaryReco { get; set; }
        public string Diagnosis { get; set; }
        public string Reason { get; set; }
        public int ReferredToMd { get; set; }
    }
}
