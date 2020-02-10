using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels.Support
{
    public partial class HospitalInfoViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Display(Name = "facility name")]
        public string FacilityName { get; set; }
        public string Abbreviation { get; set; }
        [Required]
        public int ProvinceId { get; set; }
        [Required]
        public int MuncityId { get; set; }
        [Required]
        [Display(Name = "city/municipality")]
        public int BarangayId { get; set; }
        [Display(Name = "barangay")]
        public string Address { get; set; }
        [Required]
        public string Contact { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
    }
}
