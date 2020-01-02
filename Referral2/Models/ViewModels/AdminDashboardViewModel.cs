using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public class AdminDashboardViewModel : DashboardViewModel
    {
        public AdminDashboardViewModel(int[] accepted, int[] redirected, int totalDoctor, int onlineDoctors, int activeFacilities, int referredPatients)
            : base(accepted, redirected)
        {
            TotalDoctors = totalDoctor;
            OnlineDoctors = onlineDoctors;
            ActviteFacilities = activeFacilities;
            ReferredPatients = referredPatients;
        }

        public int TotalDoctors { get; set; }
        public int OnlineDoctors { get; set; }
        public int ActviteFacilities { get; set; }
        public int ReferredPatients { get; set; }
    }
}
