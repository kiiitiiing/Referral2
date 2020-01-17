using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Doctor
{
    public partial class PatientViewModel
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public int PatientAge { get; set; }
        public string PatientSex { get; set; }
        public string PatientCivilStatus { get; set; }
        public string PatientAddress { get; set; }
        public string PatientPhicStatus { get; set; }
        public string PatientPhicId { get; set; }
    }
}
