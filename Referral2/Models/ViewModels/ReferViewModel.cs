
using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels
{
    public partial class ReferViewModel
    {
        public string Code { get; set; }
        [Required]
        public string Remarks { get; set; }
        [Required]
        public int FacilityId { get; set; }
        [Required]
        public int DepartmentId { get; set; }
    }
}
