using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Remarks
{
    public partial class TravelViewModel
    {
        [Required]
        public int TrackingId { get; set; }
        [Required]
        public int TranspoId { get; set; }
        public string Other { get; set; }
    }
}
