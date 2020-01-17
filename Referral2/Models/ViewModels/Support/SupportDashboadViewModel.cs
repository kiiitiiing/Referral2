using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Support
{
    public partial class SupportDashboadViewModel 
    {
        public SupportDashboadViewModel(int totalDoctors, int onlineDoctors, int referredPatients)
        {
            TotalDoctors = totalDoctors;
            OnlineDoctors = onlineDoctors;
            ReferredPatients = referredPatients;
        }
        public int TotalDoctors { get; set; }
        public int OnlineDoctors { get; set; }
        public int ReferredPatients { get; set; }
    }
}
