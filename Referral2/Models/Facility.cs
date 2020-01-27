using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Facility
    {
        public Facility()
        {
            ActivityReferredFromNavigation = new HashSet<Activity>();
            ActivityReferredToNavigation = new HashSet<Activity>();
            PatientFormReferredToNavigation = new HashSet<PatientForm>();
            PatientFormReferringFacility = new HashSet<PatientForm>();
            PregnantFormReferredToNavigation = new HashSet<PregnantForm>();
            PregnantFormReferringFacilityNavigation = new HashSet<PregnantForm>();
            Seen = new HashSet<Seen>();
            TrackingReferredFromNavigation = new HashSet<Tracking>();
            TrackingReferredToNavigation = new HashSet<Tracking>();
            User = new HashSet<User>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(255)]
        public string Name { get; set; }
        [StringLength(255)]
        public string Abbrevation { get; set; }
        [StringLength(255)]
        public string Address { get; set; }
        public int? BarangayId { get; set; }
        public int? MuncityId { get; set; }
        public int ProvinceId { get; set; }
        [StringLength(255)]
        public string Contact { get; set; }
        [StringLength(255)]
        public string Email { get; set; }
        public int? Status { get; set; }
        [StringLength(255)]
        public string Picture { get; set; }
        [StringLength(100)]
        public string ChiefHospital { get; set; }
        public int HospitalLevel { get; set; }
        [StringLength(20)]
        public string HospitalType { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(BarangayId))]
        [InverseProperty("Facility")]
        public virtual Barangay Barangay { get; set; }
        [ForeignKey(nameof(MuncityId))]
        [InverseProperty("Facility")]
        public virtual Muncity Muncity { get; set; }
        [ForeignKey(nameof(ProvinceId))]
        [InverseProperty("Facility")]
        public virtual Province Province { get; set; }
        [InverseProperty(nameof(Activity.ReferredFromNavigation))]
        public virtual ICollection<Activity> ActivityReferredFromNavigation { get; set; }
        [InverseProperty(nameof(Activity.ReferredToNavigation))]
        public virtual ICollection<Activity> ActivityReferredToNavigation { get; set; }
        [InverseProperty(nameof(PatientForm.ReferredToNavigation))]
        public virtual ICollection<PatientForm> PatientFormReferredToNavigation { get; set; }
        [InverseProperty(nameof(PatientForm.ReferringFacility))]
        public virtual ICollection<PatientForm> PatientFormReferringFacility { get; set; }
        [InverseProperty(nameof(PregnantForm.ReferredToNavigation))]
        public virtual ICollection<PregnantForm> PregnantFormReferredToNavigation { get; set; }
        [InverseProperty(nameof(PregnantForm.ReferringFacilityNavigation))]
        public virtual ICollection<PregnantForm> PregnantFormReferringFacilityNavigation { get; set; }
        [InverseProperty("Facility")]
        public virtual ICollection<Seen> Seen { get; set; }
        [InverseProperty(nameof(Tracking.ReferredFromNavigation))]
        public virtual ICollection<Tracking> TrackingReferredFromNavigation { get; set; }
        [InverseProperty(nameof(Tracking.ReferredToNavigation))]
        public virtual ICollection<Tracking> TrackingReferredToNavigation { get; set; }
        [InverseProperty("Facility")]
        public virtual ICollection<User> User { get; set; }
    }
}
