using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class User
    {
        public User()
        {
            ActivityActionMdNavigation = new HashSet<Activity>();
            ActivityReferringMdNavigation = new HashSet<Activity>();
            FeedbackReciever = new HashSet<Feedback>();
            FeedbackSender = new HashSet<Feedback>();
            Login = new HashSet<Login>();
            PatientFormReferredMdNavigation = new HashSet<PatientForm>();
            PatientFormReferringMdNavigation = new HashSet<PatientForm>();
            PregnantForm = new HashSet<PregnantForm>();
            Seen = new HashSet<Seen>();
            TrackingActionMdNavigation = new HashSet<Tracking>();
            TrackingReferringMdNavigation = new HashSet<Tracking>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(50)]
        public string Username { get; set; }
        [Required]
        [StringLength(255)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [StringLength(50)]
        public string Level { get; set; }
        public int FacilityId { get; set; }
        public int? DepartmentId { get; set; }
        [Required]
        [StringLength(50)]
        public string Firstname { get; set; }
        [Required]
        [StringLength(50)]
        public string Middlename { get; set; }
        [Required]
        [StringLength(50)]
        public string Lastname { get; set; }
        [StringLength(50)]
        public string Title { get; set; }
        [Required]
        [StringLength(50)]
        public string Contact { get; set; }
        [Required]
        [StringLength(50)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public int? MuncityId { get; set; }
        public int ProvinceId { get; set; }
        [StringLength(255)]
        public string AccreditationNo { get; set; }
        [StringLength(255)]
        public string AccreditationValidity { get; set; }
        [StringLength(255)]
        public string LicenseNo { get; set; }
        [StringLength(255)]
        public string Prefix { get; set; }
        [StringLength(255)]
        public string Picture { get; set; }
        [StringLength(255)]
        public string Designation { get; set; }
        [StringLength(255)]
        public string Status { get; set; }
        public DateTime LastLogin { get; set; }
        [StringLength(255)]
        public string LoginStatus { get; set; }
        [StringLength(255)]
        public string RemeberToken { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }


        [ForeignKey(nameof(DepartmentId))]
        [InverseProperty("User")]
        public virtual Department Department { get; set; }
        [ForeignKey(nameof(FacilityId))]
        [InverseProperty("User")]
        public virtual Facility Facility { get; set; }
        [ForeignKey(nameof(MuncityId))]
        [InverseProperty("User")]
        public virtual Muncity Muncity { get; set; }
        [ForeignKey(nameof(ProvinceId))]
        [InverseProperty("User")]
        public virtual Province Province { get; set; }
        [InverseProperty(nameof(Activity.ActionMdNavigation))]
        public virtual ICollection<Activity> ActivityActionMdNavigation { get; set; }
        [InverseProperty(nameof(Activity.ReferringMdNavigation))]
        public virtual ICollection<Activity> ActivityReferringMdNavigation { get; set; }
        [InverseProperty(nameof(Feedback.Reciever))]
        public virtual ICollection<Feedback> FeedbackReciever { get; set; }
        [InverseProperty(nameof(Feedback.Sender))]
        public virtual ICollection<Feedback> FeedbackSender { get; set; }
        [InverseProperty("User")]
        public virtual ICollection<Login> Login { get; set; }
        [InverseProperty(nameof(PatientForm.ReferredMdNavigation))]
        public virtual ICollection<PatientForm> PatientFormReferredMdNavigation { get; set; }
        [InverseProperty(nameof(PatientForm.ReferringMdNavigation))]
        public virtual ICollection<PatientForm> PatientFormReferringMdNavigation { get; set; }
        [InverseProperty("ReferredByNavigation")]
        public virtual ICollection<PregnantForm> PregnantForm { get; set; }
        [InverseProperty("UserMdNavigation")]
        public virtual ICollection<Seen> Seen { get; set; }
        [InverseProperty(nameof(Tracking.ActionMdNavigation))]
        public virtual ICollection<Tracking> TrackingActionMdNavigation { get; set; }
        [InverseProperty(nameof(Tracking.ReferringMdNavigation))]
        public virtual ICollection<Tracking> TrackingReferringMdNavigation { get; set; }
    }
}
