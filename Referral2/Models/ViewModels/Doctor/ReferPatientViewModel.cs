﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class ReferPatientViewModel : PatientViewModel
    {
        public string ReferringFacility { get; set; }
        public string ReferringFacilityAddress { get; set; }

        //Send form
        public int ReferredTo { get; set; }
        public int Department { get; set; }
        public string CaseSummary { get; set; }
        public string SummaryReco { get; set; }
        public string Diagnosis { get; set; }
        public string Reason { get; set; }
        public string ReferringMd { get; set; }
        public int ReferredToMd { get; set; }
    }
}
