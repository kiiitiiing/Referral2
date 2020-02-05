using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Mcc
{
    public partial class MccIncomingViewModel
    {
        public string Facility { get; set; }
        public int AcceptedCount { get; set; }
        public int RedirectedCount { get; set; }
        public int SeenCount { get; set; }
        public int IdleCount { get; set; }
        public int NoActionCount { get; set; }
        public int Total { get; set; }
    }
}
