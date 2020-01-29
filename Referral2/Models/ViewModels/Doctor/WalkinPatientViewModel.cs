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
        [Display(Name = "facility")]
        public int ReferringFacility { get; set; }
        [Required]
        [Display(Name = "department")]
        public int Department { get; set; }
        [Required]
        [Display(Name = "case summary")]
        public string CaseSummary { get; set; }
        [Required]
        [Display(Name = "summary of reco")]
        public string SummaryReco { get; set; }
        [Required]
        [Display(Name = "diagnosis/impression")]
        public string Diagnosis { get; set; }
        [Required]
        [Display(Name = "reason")]
        public string Reason { get; set; }
        public int? ReferredToMd { get; set; }
    }
}
