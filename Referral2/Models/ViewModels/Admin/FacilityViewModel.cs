using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class FacilityViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "abbrevation")]
        public string Abbrevation { get; set; }
        [Required]
        [Display(Name = "province")]
        public int? Province { get; set; }
        [Required]
        [Display(Name = "city/municipality")]
        public int? Muncity { get; set; }
        [Required]
        [Display(Name = "barangay")]
        public int? Barangay { get; set; }
        [Required]
        [Display(Name = "address")]
        public string Address { get; set; }
        [Required]
        [Display(Name = "contact")]
        public string Contact { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [Display(Name = "chief")]
        public string Chief { get; set; }
        [Required]
        [Display(Name = "level")]
        public int? Level { get; set; }
        [Required]
        [Display(Name = "type")]
        public string Type { get; set; }
    }
}
