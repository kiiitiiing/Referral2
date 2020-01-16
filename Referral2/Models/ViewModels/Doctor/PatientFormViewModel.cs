using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class PatientFormViewModel
    {
        public int PatientId { get; set; }
        public int FacilityId { get; set; }
        public int DepartmentId { get; set; }
        public string CaseSummary { get; set; }
        public string SummaryReCo { get; set; }
        public string Diagnosis { get; set; }
        public string Reason { get; set; }
        public int ReferredMd { get; set; }
    }
}
