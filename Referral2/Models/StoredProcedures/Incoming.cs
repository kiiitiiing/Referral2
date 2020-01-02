using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.StoredProcedures
{
    public class Incoming
    {
        public string Code { get; set; }
        public int PatientId { get; set; }
        public string Status { get; set; }
        public int ReferringMd { get; set; }
        public int ActionMd { get; set; }
        public string Type { get; set; }
        public DateTime DateAction { get; set; }
        public int ReferredFrom { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual Facility ReferredFromNavigation { get; set; }
        public virtual User ReferringMdNavigation { get; set; }
        public virtual User ActionMdNavigation { get; set; }

    }
}
