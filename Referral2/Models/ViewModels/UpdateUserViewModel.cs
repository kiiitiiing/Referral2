using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels
{
    public partial class UpdateUserViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "first name")]
        public string Firstname { get; set; }
        [Required]
        [Display(Name = "middle name")]
        public string Middlename { get; set; }
        [Required]
        [Display(Name = "last name")]
        public string Lastname { get; set; }
        [Required]
        [Display(Name = "contact number")]
        public string ContactNumber { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [Display(Name = "designation")]
        public string Designation { get; set; }
        [Required]
        public string Username { get; set; }
        [Display(Name = "password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password")]
        [Display(Name = "confirm password")]
        public string ConfirmPassword { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
