using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class IncomingReportViewModel
    {
        public int ReferredTo { get; set; }
        public int ReferredFrom { get; set; }
        public int Department { get; set; }
        public string Code { get; set; }
        public string Facility { get; set; }
        public DateTime DateReferred { get; set; }
        public DateTime DateArrived { get; set; }
        public DateTime DateAdmitted { get; set; }
        public DateTime DateDischarged { get; set; }
        public DateTime DateTransferred { get; set; }
        public DateTime DateCancelled { get; set; }
    }

}
