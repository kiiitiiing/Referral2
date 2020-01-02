using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Muncity
    {
        public Muncity()
        {
            Barangay = new HashSet<Barangay>();
            Facility = new HashSet<Facility>();
            Patient = new HashSet<Patient>();
            User = new HashSet<User>();
        }

        [Key]
        public int Id { get; set; }
        public int ProvinceId { get; set; }
        [Required]
        [StringLength(50)]
        public string Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ProvinceId))]
        [InverseProperty("Muncity")]
        public virtual Province Province { get; set; }
        [InverseProperty("Muncity")]
        public virtual ICollection<Barangay> Barangay { get; set; }
        [InverseProperty("Muncity")]
        public virtual ICollection<Facility> Facility { get; set; }
        [InverseProperty("Muncity")]
        public virtual ICollection<Patient> Patient { get; set; }
        [InverseProperty("Muncity")]
        public virtual ICollection<User> User { get; set; }
    }
}
