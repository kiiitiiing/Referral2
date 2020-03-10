using System;

using System.ComponentModel.DataAnnotations;

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
        [MinLength(6, ErrorMessage = "Password minimum length must be 6 characters.")]
        public string NewPassword { get; set; }
        [Required]
        [Compare("NewPassword")]
        [DataType(DataType.Password)]
        [Display(Name = "confirm password")]
        [MinLength(6, ErrorMessage = "Password minimum length must be 6 characters.")]
        public string ConfirmPassword { get; set; }
    }
}
