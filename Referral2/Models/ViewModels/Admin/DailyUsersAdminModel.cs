using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class DailyUsersAdminModel
    {
        private int _offlineHP;
        private int _offlineIT;
        public string Facility { get; set; }
        public int OnDutyHP { get; set; }
        public int OffDutyHP { get; set; }
        public int OfflineHP { get { return _offlineHP; } set { _offlineHP = value - (OnDutyHP + OffDutyHP); } }
        public int SubTotalHP { get { return OnDutyHP + OfflineHP + OfflineHP; } }
        public int OnlineIT { get; set; }
        public int OfflineIT { get { return _offlineIT; } set { _offlineIT = value - OnlineIT; } }
        public int SubTotalIT { get { return OfflineIT + OnlineIT; } }
        public int Total { get { return SubTotalHP + SubTotalIT; } }
    }
}
