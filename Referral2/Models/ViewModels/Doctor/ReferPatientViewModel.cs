using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class ReferPatientViewModel
    {
        [Required]
        public int PatientId { get; set; }
        //Send form
        [Required]
        [Display(Name ="facility")]
        public int ReferredTo { get; set; }
        [Required]
        public int Department { get; set; }
        [Required]
        [Display(Name ="case summary")]
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
