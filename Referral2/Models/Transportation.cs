using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Referral2.Models
{
    public partial class Transportation
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Column("Transportation")]
        [StringLength(255)]
        public string Transportation1 { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
