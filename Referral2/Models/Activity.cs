using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Activity
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Code { get; set; }
        public int PatientId { get; set; }
        public DateTime DateReferred { get; set; }
        public DateTime DateSeen { get; set; }
        public int? ReferredFrom { get; set; }
        public int? ReferredTo { get; set; }
        public int? DepartmentId { get; set; }
        public int? ReferringMd { get; set; }
        public int? ActionMd { get; set; }
        [Column(TypeName = "text")]
        public string Remarks { get; set; }
        [StringLength(255)]
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        [ForeignKey(nameof(ActionMd))]
        [InverseProperty(nameof(User.ActivityActionMdNavigation))]
        public virtual User ActionMdNavigation { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        [InverseProperty("Activity")]
        public virtual Department Department { get; set; }
        [ForeignKey(nameof(PatientId))]
        [InverseProperty("Activity")]
        public virtual Patient Patient { get; set; }
        [ForeignKey(nameof(ReferredFrom))]
        [InverseProperty(nameof(Facility.ActivityReferredFromNavigation))]
        public virtual Facility ReferredFromNavigation { get; set; }
        [ForeignKey(nameof(ReferredTo))]
        [InverseProperty(nameof(Facility.ActivityReferredToNavigation))]
        public virtual Facility ReferredToNavigation { get; set; }
        [ForeignKey(nameof(ReferringMd))]
        [InverseProperty(nameof(User.ActivityReferringMdNavigation))]
        public virtual User ReferringMdNavigation { get; set; }
    }
}
