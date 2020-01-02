using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Patient
    {
        public Patient()
        {
            Activity = new HashSet<Activity>();
            BabyBabyNavigation = new HashSet<Baby>();
            BabyMother = new HashSet<Baby>();
            PatientForm = new HashSet<PatientForm>();
            PregnantFormPatientBaby = new HashSet<PregnantForm>();
            PregnantFormPatientWoman = new HashSet<PregnantForm>();
            Tracking = new HashSet<Tracking>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string UniqueId { get; set; }
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
        [Required]
        [StringLength(50)]
        public string MiddleName { get; set; }
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        [Column(TypeName = "date")]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        [Required]
        [StringLength(50)]
        public string Sex { get; set; }
        [Required]
        [StringLength(50)]
        public string CivilStatus { get; set; }
        [StringLength(50)]
        public string PhicId { get; set; }
        [StringLength(50)]
        public string PhicStatus { get; set; }
        [StringLength(50)]
        public string Address { get; set; }
        public int? BarangayId { get; set; }
        public int? MuncityId { get; set; }
        public int? ProvinceId { get; set; }
        public int? TsekapPatient { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(BarangayId))]
        [InverseProperty("Patient")]
        public virtual Barangay Barangay { get; set; }
        [ForeignKey(nameof(MuncityId))]
        [InverseProperty("Patient")]
        public virtual Muncity Muncity { get; set; }
        [ForeignKey(nameof(ProvinceId))]
        [InverseProperty("Patient")]
        public virtual Province Province { get; set; }
        [InverseProperty("Patient")]
        public virtual ICollection<Activity> Activity { get; set; }
        [InverseProperty(nameof(Baby.BabyNavigation))]
        public virtual ICollection<Baby> BabyBabyNavigation { get; set; }
        [InverseProperty(nameof(Baby.Mother))]
        public virtual ICollection<Baby> BabyMother { get; set; }
        [InverseProperty("Patient")]
        public virtual ICollection<PatientForm> PatientForm { get; set; }
        [InverseProperty(nameof(PregnantForm.PatientBaby))]
        public virtual ICollection<PregnantForm> PregnantFormPatientBaby { get; set; }
        [InverseProperty(nameof(PregnantForm.PatientWoman))]
        public virtual ICollection<PregnantForm> PregnantFormPatientWoman { get; set; }
        [InverseProperty("Patient")]
        public virtual ICollection<Tracking> Tracking { get; set; }
    }
}
