using System;
using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class WalkinPatientViewModel
    {
        //Send form
        [Required]
        public int PatientId { get; set; }
        [Required]
        public int ReferringFacility { get; set; }
        [Required]
        public int Department { get; set; }
        [Required]
        public string CaseSummary { get; set; }
        [Required]
        public string SummaryReco { get; set; }
        [Required]
        public string Diagnosis { get; set; }
        [Required]
        public string Reason { get; set; }
        public int? ReferredToMd { get; set; }
    }
}
