namespace Referral2.Models.ViewModels.Support
{
    public partial class DailyReferralSupport
    {
        public string DoctorName { get; set; }
        public int OutAccepted { get; set; }
        public int OutRedirected { get; set; }
        public int OutSeen { get; set; }
        public int OutTotal { get; set; }
        public int InAccepted { get; set; }
        public int InRedirected { get; set; }
        public int InSeen { get; set; }
    }
}
