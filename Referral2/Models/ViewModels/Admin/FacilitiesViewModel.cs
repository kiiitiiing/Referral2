using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class FacilitiesViewModel
    {
        public int Id { get; set; }
        public string Facility { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Email { get; set; }
        public string Chief { get; set; }
        public int Level { get; set; }
        public string Type { get; set; }
    }
}
