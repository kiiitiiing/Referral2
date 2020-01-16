using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Mcc
{
    public partial class UserDepartmentViewModel
    {
        public string Department { get; set; }
        public int OnDuty { get; set; }
        public int OffDuty { get; set; }
        public int NumberOfUsers { get; set; }
    }
}
