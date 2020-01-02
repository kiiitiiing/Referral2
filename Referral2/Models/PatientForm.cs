using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class PatientForm
    {
        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string UniqueId { get; set; }
        [StringLength(50)]
        public string Code { get; set; }
        public int? ReferringFacilityId { get; set; }
        public int? ReferredTo { get; set; }
        public int? DepartmentId { get; set; }
        public DateTime TimeReferred { get; set; }
        public DateTime TimeTransferred { get; set; }
        public int PatientId { get; set; }
        [Column(TypeName = "text")]
        public string CaseSummary { get; set; }
        [Column(TypeName = "text")]
        public string RecommendSummary { get; set; }
        [StringLength(255)]
        public string Diagnosis { get; set; }
        [StringLength(255)]
        public string Reason { get; set; }
        public int? ReferringMd { get; set; }
        public int? ReferredMd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        [InverseProperty("PatientForm")]
        public virtual Department Department { get; set; }
        [ForeignKey(nameof(PatientId))]
        [InverseProperty("PatientForm")]
        public virtual Patient Patient { get; set; }
        [ForeignKey(nameof(ReferredMd))]
        [InverseProperty(nameof(User.PatientFormReferredMdNavigation))]
        public virtual User ReferredMdNavigation { get; set; }
        [ForeignKey(nameof(ReferredTo))]
        [InverseProperty(nameof(Facility.PatientFormReferredToNavigation))]
        public virtual Facility ReferredToNavigation { get; set; }
        [ForeignKey(nameof(ReferringFacilityId))]
        [InverseProperty(nameof(Facility.PatientFormReferringFacility))]
        public virtual Facility ReferringFacility { get; set; }
        [ForeignKey(nameof(ReferringMd))]
        [InverseProperty(nameof(User.PatientFormReferringMdNavigation))]
        public virtual User ReferringMdNavigation { get; set; }
    }
}
