using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class PregnantForm
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string UniqueId { get; set; }
        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        public int ReferringFacility { get; set; }
        public int ReferredBy { get; set; }
        [StringLength(50)]
        public string RecordNo { get; set; }
        public DateTime ReferredDate { get; set; }
        public int ReferredTo { get; set; }
        public int DepartmentId { get; set; }
        public DateTime ArrivalDate { get; set; }
        [StringLength(50)]
        public string HealthWorker { get; set; }
        public int PatientWomanId { get; set; }
        [StringLength(255)]
        public string WomanReason { get; set; }
        [Column(TypeName = "text")]
        public string WomanMajorFindings { get; set; }
        [StringLength(255)]
        public string WomanBeforeTreatment { get; set; }
        public DateTime? WomanBeforeGivenTime { get; set; }
        [StringLength(255)]
        public string WomanDuringTransport { get; set; }
        public DateTime? WomanTransportGivenTime { get; set; }
        [Column(TypeName = "text")]
        public string WomanInformationGiven { get; set; }
        public int? PatientBabyId { get; set; }
        [StringLength(255)]
        public string BabyReason { get; set; }
        [StringLength(255)]
        public string BabyMajorFindings { get; set; }
        public DateTime? BabyLastFeed { get; set; }
        [StringLength(255)]
        public string BabyBeforeTreatment { get; set; }
        public DateTime? BabyBeforeGivenTime { get; set; }
        [StringLength(255)]
        public string BabyDuringTransport { get; set; }
        public DateTime? BabyTransportGivenTime { get; set; }
        [StringLength(255)]
        public string BabyInformationGiven { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        [InverseProperty("PregnantForm")]
        public virtual Department Department { get; set; }
        [ForeignKey(nameof(PatientBabyId))]
        [InverseProperty(nameof(Patient.PregnantFormPatientBaby))]
        public virtual Patient PatientBaby { get; set; }
        [ForeignKey(nameof(PatientWomanId))]
        [InverseProperty(nameof(Patient.PregnantFormPatientWoman))]
        public virtual Patient PatientWoman { get; set; }
        [ForeignKey(nameof(ReferredBy))]
        [InverseProperty(nameof(User.PregnantForm))]
        public virtual User ReferredByNavigation { get; set; }
        [ForeignKey(nameof(ReferredTo))]
        [InverseProperty(nameof(Facility.PregnantFormReferredToNavigation))]
        public virtual Facility ReferredToNavigation { get; set; }
        [ForeignKey(nameof(ReferringFacility))]
        [InverseProperty(nameof(Facility.PregnantFormReferringFacilityNavigation))]
        public virtual Facility ReferringFacilityNavigation { get; set; }
    }
}
