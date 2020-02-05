using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public class DailyUsersAdminModel
    {
        public string Facility { get; set; }
        public int OnDutyHP { get; set; }
        public int OffDutyHP { get; set; }
        public int OfflineHP { get; set; }
        public int OnlineIT { get; set; }
        public int OfflineIT { get; set; }
    }
}
