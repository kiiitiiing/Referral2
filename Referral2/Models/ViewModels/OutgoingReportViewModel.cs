using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class OutgoingReportViewModel
    {
        public string Code { get; set; }
        public DateTime DateReferred { get; set; }
        public TimeSpan Seen { get; set; }
        public TimeSpan Accepted { get; set; }
        public TimeSpan Arrived { get; set; }
        public TimeSpan Redirected { get; set; }
        public TimeSpan NoAction { get; set; }
    }
}
