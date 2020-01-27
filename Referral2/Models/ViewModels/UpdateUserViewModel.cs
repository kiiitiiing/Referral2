using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels
{
    public partial class UpdateUserViewModel
    {
        public int Id { get; set; }
        public string Firstname { get; set; }
        public string Middlename { get; set; }
        public string Lastname { get; set; }
        public string ContactNumber { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Designation { get; set; }
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
        public string Status { get; set; }
    }
}
