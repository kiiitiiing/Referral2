using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    
    public partial class Barangay
    {
        public Barangay()
        {
            Facility = new HashSet<Facility>();
            Patient = new HashSet<Patient>();
        }

        [Key]
        public int Id { get; set; }
        public int ProvinceId { get; set; }
        public int MuncityId { get; set; }
        [Required]
        [StringLength(255)]
        public string Description { get; set; }
        public int OldTarget { get; set; }
        public int Target { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(MuncityId))]
        [InverseProperty("Barangay")]
        public virtual Muncity Muncity { get; set; }
        [ForeignKey(nameof(ProvinceId))]
        [InverseProperty("Barangay")]
        public virtual Province Province { get; set; }
        [InverseProperty("Barangay")]
        public virtual ICollection<Facility> Facility { get; set; }
        [InverseProperty("Barangay")]
        public virtual ICollection<Patient> Patient { get; set; }
    }
}
