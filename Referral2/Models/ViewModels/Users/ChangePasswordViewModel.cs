using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Users
{
    public partial class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "current password")]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "New Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "new password")]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        [Display(Name = "confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
