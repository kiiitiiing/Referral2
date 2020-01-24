using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class ReferPatientViewModel
    {
        [Required]
        public int PatientId { get; set; }
        //Send form
        [Required]
        public int ReferredTo { get; set; }
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
