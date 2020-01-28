using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.MobileModels
{
    public class NotificationModel
    {
        public string PatientName { get; set; }
        public string PatientCode { get; set; }
        public string TrackStatus { get; set; }
        public string DisplayNotification { get; set; }
        public string ReferringDoctor { get; set; }
    }
}
