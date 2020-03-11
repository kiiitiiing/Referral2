using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Issue
    {
        [Key]
        public int Id { get; set; }
        public int TrackingId { get; set; }
        [Required]
        [Column("Issue")]
        [StringLength(255)]
        public string Issue1 { get; set; }
        [Required]
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(TrackingId))]
        [InverseProperty("Issue")]
        public virtual Tracking Tracking { get; set; }
    }
}
