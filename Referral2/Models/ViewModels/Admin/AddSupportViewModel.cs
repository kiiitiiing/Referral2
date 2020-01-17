using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class AddSupportViewModel : AddUserViewModel
    {
        [Required]
        public int FacilityId { get; set; }
    }
}
