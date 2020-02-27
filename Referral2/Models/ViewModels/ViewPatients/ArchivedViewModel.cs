
using System;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public partial class ArchivedViewModel
    {
        public string ReferringFacility { get; set; }
        public string Type { get; set; }
        public string PatientName { get; set; }
        public string Code { get; set; }
        public DateTime DateArchive { get; set; } 
        public string Reason { get; set; }
    }
}
