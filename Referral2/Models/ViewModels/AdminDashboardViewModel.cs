using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class AdminDashboardViewModel
    {
        public AdminDashboardViewModel(int totalDoctor, int onlineDoctors, int activeFacilities, int referredPatients)
        {
            TotalDoctors = totalDoctor;
            OnlineDoctors = onlineDoctors;
            ActviteFacilities = activeFacilities;
            ReferredPatients = referredPatients;
        }

        public AdminDashboardViewModel(int totalDoctor, int onlineDoctors, int referredPatients)
        {
            TotalDoctors = totalDoctor;
            OnlineDoctors = onlineDoctors;
            ReferredPatients = referredPatients;
        }

        public int TotalDoctors { get; set; }
        public int OnlineDoctors { get; set; }
        public int ActviteFacilities { get; set; }
        public int ReferredPatients { get; set; }
    }
}
