using System;

namespace Referral2.Models.ViewModels.Mcc
{
    public partial class TimeFrameViewModel
    {
        public string Code { get; set; }
        public DateTime TimeReferred { get; set; }
        public double Seen { get; set; }
        public double Accepted { get; set; }
        public double Arrived { get; set; }
        public double Redirected { get; set; }
    }
}
