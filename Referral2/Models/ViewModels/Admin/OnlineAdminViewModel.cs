using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class OnlineAdminViewModel
    {
        public int UserId { get; set; }
        public string FacilityName { get; set; }
        public string UserFullName { get; set; }
        public string UserLevel { get; set; }
        public string UserDepartment { get; set; }
        public string UserStatus { get; set; }
        public DateTime UserLoginTime { get; set; }
    }
}
