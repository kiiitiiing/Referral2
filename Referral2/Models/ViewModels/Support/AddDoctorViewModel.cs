using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Support
{
    public partial class AddDoctorViewModel : AddUserViewModel
    {
        [Display(Name ="Department")]
        public int? Department { get; set; }
        [Required]
        public string Level { get; set; }
    }
}
