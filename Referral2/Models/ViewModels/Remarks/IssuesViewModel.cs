using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Remarks
{
    public partial class IssuesViewModel
    {
        public int TrackingId { get; set; }
        public List<string> Issue { get; set; }
    }
}
