using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class SubOnlineViewModel
    {
        public string Name { get; set; }
        public string Level { get; set; }
        public string Department { get; set; }
        public string Status { get; set; }
        public DateTime Login { get; set; }
    }
}
