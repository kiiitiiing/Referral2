using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Seen
    {
        [Key]
        public int Id { get; set; }
        public int TrackingId { get; set; }
        public int FacilityId { get; set; }
        public int UserMd { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(FacilityId))]
        [InverseProperty("Seen")]
        public virtual Facility Facility { get; set; }
        [ForeignKey(nameof(TrackingId))]
        [InverseProperty("Seen")]
        public virtual Tracking Tracking { get; set; }
        [ForeignKey(nameof(UserMd))]
        [InverseProperty(nameof(User.Seen))]
        public virtual User UserMdNavigation { get; set; }
    }
}
