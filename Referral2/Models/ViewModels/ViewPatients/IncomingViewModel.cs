using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public partial class IncomingViewModel
    {
        public bool Pregnant { get; set; }
        public int TrackingId { get; set; }
        public string Code { get; set; }
        public string PatientName { get; set; }
        public string PatientSex { get; set; }
        public int PatientAge { get; set; }
        public string Status { get; set; }
        public int SeenCount { get; set; }
        public int CallCount { get; set; }
        public int FeedbackCount { get; set; }
        public string ReferringMd { get; set; }
        public string ActionMd { get; set; }
        public DateTime DateAction { get; set; }
        public string ReferredFrom { get; set; }
        public int ReferredFromId { get; set; }
        public string ReferredTo { get; set; }
        public int ReferredToId { get; set; }
        public string Department { get; set; }
        public int DepartmentId { get; set; }
    }
}
