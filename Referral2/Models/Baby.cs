using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Baby
    {
        [Key]
        public int Id { get; set; }
        public int? BabyId { get; set; }
        public int MotherId { get; set; }
        public int Weight { get; set; }
        public int GestationalAge { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(BabyId))]
        [InverseProperty(nameof(Patient.BabyBabyNavigation))]
        public virtual Patient BabyNavigation { get; set; }
        [ForeignKey(nameof(MotherId))]
        [InverseProperty(nameof(Patient.BabyMother))]
        public virtual Patient Mother { get; set; }
    }
}
