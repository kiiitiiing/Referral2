using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class UpdateSupportViewModel : UpdateUserViewModel
    {
        [Required]
        public int Facility { get; set; }
    }
}
