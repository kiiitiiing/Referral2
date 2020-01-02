using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Tracking
    {
        public Tracking()
        {
            Issue = new HashSet<Issue>();
            Seen = new HashSet<Seen>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        public string Code { get; set; }
        public int PatientId { get; set; }
        public DateTime DateReferred { get; set; }
        public DateTime DateTransferred { get; set; }
        public DateTime DateAccepted { get; set; }
        public DateTime DateArrived { get; set; }
        public DateTime DateSeen { get; set; }
        [StringLength(50)]
        public string Transportation { get; set; }
        public int? ReferredFrom { get; set; }
        public int? ReferredTo { get; set; }
        public int? DepartmentId { get; set; }
        public int? ReferringMd { get; set; }
        public int? ActionMd { get; set; }
        [Column(TypeName = "text")]
        public string Remarks { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        [StringLength(50)]
        public string Type { get; set; }
        [StringLength(255)]
        public string WalkIn { get; set; }
        public int? FormId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(ActionMd))]
        [InverseProperty(nameof(User.TrackingActionMdNavigation))]
        public virtual User ActionMdNavigation { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        [InverseProperty("Tracking")]
        public virtual Department Department { get; set; }
        [ForeignKey(nameof(PatientId))]
        [InverseProperty("Tracking")]
        public virtual Patient Patient { get; set; }
        [ForeignKey(nameof(ReferredFrom))]
        [InverseProperty(nameof(Facility.TrackingReferredFromNavigation))]
        public virtual Facility ReferredFromNavigation { get; set; }
        [ForeignKey(nameof(ReferredTo))]
        [InverseProperty(nameof(Facility.TrackingReferredToNavigation))]
        public virtual Facility ReferredToNavigation { get; set; }
        [ForeignKey(nameof(ReferringMd))]
        [InverseProperty(nameof(User.TrackingReferringMdNavigation))]
        public virtual User ReferringMdNavigation { get; set; }
        [InverseProperty("Tracking")]
        public virtual ICollection<Issue> Issue { get; set; }
        [InverseProperty("Tracking")]
        public virtual ICollection<Seen> Seen { get; set; }
    }
}
