using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class FacilityViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Abbrevation { get; set; }
        [Required]
        public int? Province { get; set; }
        [Required]
        public int? Muncity { get; set; }
        [Required]
        public int? Barangay { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string Contact { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Chief { get; set; }
        [Required]
        public int? Level { get; set; }
        [Required]
        public string Type { get; set; }
    }
}
