using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels.Account
{
    public partial class SwitchUserModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
