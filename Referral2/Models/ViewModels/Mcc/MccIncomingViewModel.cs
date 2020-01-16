using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Mcc
{
    public partial class MccIncomingViewModel
    {
        public MccIncomingViewModel(string facility, int acceptedCount, int redirectedCount, int idleCount, int noActionCount)
        {
            Facility = facility;
            AcceptedCount = acceptedCount;
            RedirectedCount = redirectedCount;
            IdleCount = IdleCount;
            NoActionCount = noActionCount;
            Total = acceptedCount + redirectedCount + idleCount + noActionCount;
        }
        public string Facility { get; set; }
        public int AcceptedCount { get; set; }
        public int RedirectedCount { get; set; }
        public int IdleCount { get; set; }
        public int NoActionCount { get; set; }
        public int Total { get; set; }
    }
}
