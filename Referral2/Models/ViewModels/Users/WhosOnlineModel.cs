using System;

namespace Referral2.Models.ViewModels.Users
{
    public partial class WhosOnlineModel
    {
        public string DoctorName { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbrv { get; set; }
        public string Contact { get; set; }
        public string Department { get; set; }
        public bool LoginStatus { get; set; }
        public DateTime LoginTime { get; set; }
    }
}
