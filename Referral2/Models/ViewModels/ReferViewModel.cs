using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class ReferViewModel
    {
        public string Code { get; set; }
        public string Remarks { get; set; }
        public int FacilityId { get; set; }
        public int DepartmentId { get; set; }
    }
}
