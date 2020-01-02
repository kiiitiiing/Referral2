using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Doctor
{
    public class PregnantFormViewModel
    {
        public int FacilityId { get; set; }
        public int DepartmentId { get; set; }
        public string RecordNp { get; set; }
        public string HealthWorker { get; set; }
        public string WomanMainReason { get; set; }
        public string WomanMajorFindings { get; set; }
        public string WomanBeforeTreatmentGiven { get; set; }
        public DateTime WomanBeforeDateTimeGiven { get; set; }
        public string WomanDuringTreatmentGiven { get; set; }
        public DateTime WomanDureingDateTimeGiven { get; set; }
        public string WomanInformationGiven { get; set; }
        public string BabyFirstName { get; set; }
        public string BabyMiddleName { get; set; }
        public string BabyLastName { get; set; }
        public DateTime BabyDateOfBirth { get; set; }
        public int? BabyBirthWeight { get; set; }
        public int? GestationalAge { get; set; }
        public string BabyMainReason { get; set; }
        public string BabyMajorFindings { get; set; }
        public DateTime BabyLastFeed { get; set; }
        public string BabyBeforeTreatmentGiven { get; set; }
        public DateTime BabyBeforeDateTimeGiven { get; set; }
        public string BabyDuringTreatmentGiven { get; set; }
        public DateTime BabyDuringDateTimeGiven { get; set; }
        public string BabyInformationGiven { get; set; }
    }
}
