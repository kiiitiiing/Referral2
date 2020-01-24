using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class PregnantReferViewModel
    {
        [Required]
        [Display(Name ="Facility")]
        public int ReferredTo { get; set; }
        [Required]
        [Display(Name = "Department")]
        public int Department { get; set; }
        //[Required]
        public string WomanMajorFindings { get; set; }
        [Required]
        public int PatientId { get; set; }
        //[Required]
        public string WomanInformationGiven { get; set; }
        public string RecordNumber { get; set; }
        public string HealthWorker { get; set; }
        public string WomanMainReason { get; set; }
        public string WomanBeforeTreatmentGiven { get; set; }
        public DateTime? WomanBeforeDateTimeGiven { get; set; }
        public string WomanDuringTreatmentGiven { get; set; }
        public DateTime? WomanDuringDateTimeGiven { get; set; }
        public string BabyFirstName { get; set; }
        public string BabyMiddleName { get; set; }
        public string BabyLastName { get; set; }
        public DateTime? BabyDateOfBirth { get; set; }
        public int? BabyBirthWeight { get; set; }
        public int? GestationalAge { get; set; }
        public string BabyMainReason { get; set; }
        public string BabyMajorFindings { get; set; }
        public DateTime? BabyLastFeed { get; set; }
        public string BabyBeforeTreatmentGiven { get; set; }
        public DateTime? BabyBeforeDateTimeGiven { get; set; }
        public string BabyDuringTreatmentGiven { get; set; }
        public DateTime? BabyDuringDateTimeGiven { get; set; }
        public string BabyInformationGiven { get; set; }
    }
}
