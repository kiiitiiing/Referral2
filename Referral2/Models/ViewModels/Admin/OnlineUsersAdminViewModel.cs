using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class OnlineUsersAdminViewModel
    {
        public string Facility { get; set; }
        public IEnumerable<SubOnlineViewModel> LoggedInUser { get; set; }
    }
}
