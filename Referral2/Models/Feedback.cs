using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Feedback
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(255)]
        public string Code { get; set; }
        public int? SenderId { get; set; }
        public int? RecieverId { get; set; }
        [StringLength(255)]
        public string Message { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(RecieverId))]
        [InverseProperty(nameof(User.FeedbackReciever))]
        public virtual User Reciever { get; set; }
        [ForeignKey(nameof(SenderId))]
        [InverseProperty(nameof(User.FeedbackSender))]
        public virtual User Sender { get; set; }
    }
}
