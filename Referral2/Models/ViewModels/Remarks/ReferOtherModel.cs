using System.ComponentModel.DataAnnotations;
namespace Referral2.Models.ViewModels.Remarks
{
    public partial class ReferOtherModel
    {
        public string Code { get; set; }
        [Required]
        public int FacilityId { get; set; }
        [Required]
        public int DepartmentId { get; set; }
    }
}
