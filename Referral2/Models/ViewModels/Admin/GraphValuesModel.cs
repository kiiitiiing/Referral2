using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class GraphValuesModel
    {
        public string Facility { get; set; }
        public int Incoming { get; set; }
        public int Accepted { get; set; }
        public int Outgoing { get; set; }
        public int Total { get { return Incoming + Accepted + Outgoing; } }
    }
}
