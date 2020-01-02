using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Support
{
    public class SupportDashboadViewModel : DashboardViewModel
    {
        public SupportDashboadViewModel(int[] accepted, int[] redirected, int totalDoctor, int onlineDoctors, int referredPatients)
            : base(accepted, redirected)
        {
            TotalDoctors = totalDoctor;
            OnlineDoctors = onlineDoctors;
            ReferredPatients = referredPatients;
        }

        public int TotalDoctors { get; set; }
        public int OnlineDoctors { get; set; }
        public int ReferredPatients { get; set; }
    }
}
