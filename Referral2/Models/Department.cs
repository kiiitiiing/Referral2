using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Department
    {
        public Department()
        {
            Activity = new HashSet<Activity>();
            PatientForm = new HashSet<PatientForm>();
            PregnantForm = new HashSet<PregnantForm>();
            Tracking = new HashSet<Tracking>();
            User = new HashSet<User>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Description { get; set; }

        [InverseProperty("Department")]
        public virtual ICollection<Activity> Activity { get; set; }
        [InverseProperty("Department")]
        public virtual ICollection<PatientForm> PatientForm { get; set; }
        [InverseProperty("Department")]
        public virtual ICollection<PregnantForm> PregnantForm { get; set; }
        [InverseProperty("Department")]
        public virtual ICollection<Tracking> Tracking { get; set; }
        [InverseProperty("Department")]
        public virtual ICollection<User> User { get; set; }
    }
}
