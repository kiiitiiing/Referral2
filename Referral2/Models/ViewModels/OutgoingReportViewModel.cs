using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class OutgoingReportViewModel
    {
        public int Department { get; set; }
        public int ReferredTo { get; set; }
        public int ReferredFrom { get; set; }
        public string Code { get; set; }
        public DateTime DateReferred { get; set; }
        public double Seen { get; set; }
        public double Accepted { get; set; }
        public double Arrived { get; set; }
        public double Redirected { get; set; }
        public double NoAction { get; set; }
    }
}
