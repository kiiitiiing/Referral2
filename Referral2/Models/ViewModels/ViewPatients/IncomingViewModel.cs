using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public class IncomingViewModel
    {
        public string Code { get; set; }
        public Patient Patient { get; set; }
        public string Status { get; set; }
        public User ReferringMd { get; set; }
        public User ActionMd { get; set; }
        public DateTime DateAction { get; set; }
        public Facility ReferredFrom { get; set; }
        public Facility ReferredTo { get; set; }
        public Department DepartmentId { get; set; }
    }
}
