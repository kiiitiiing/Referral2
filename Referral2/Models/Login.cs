using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Login
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [Column("Login")]
        public DateTime Login1 { get; set; }
        public DateTime Logout { get; set; }
        [Required]
        [StringLength(255)]
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty("Login")]
        public virtual User User { get; set; }
    }
}
