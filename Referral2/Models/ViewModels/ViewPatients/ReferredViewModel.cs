using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.ViewPatients
{
    public class ReferredViewModel
    {
        public Facility ReferredFrom { get; set; }
        public Facility ReferredTo { get; set; }
        public Department Department { get; set; }
        public string Code { get; set; }
        public Patient Patient { get; set; }
        public User ReferringMd { get; set; }
        public User ActionMd { get; set; }
        public IEnumerable<Activity> Activities { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}
