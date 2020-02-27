namespace Referral2.Models.ViewModels.Support
{
    public partial class DailyReferralSupport
    {
        private int _seenOut;
        private int _seenIn;
        public string DoctorName { get; set; }
        public int OutAccepted { get; set; }
        public int OutRedirected { get; set; }
        private int AccRedOut { get { return OutAccepted + OutRedirected; } }
        public int OutSeen { get { return _seenOut > 0 ? _seenOut : 0; } set { _seenOut = value - AccRedOut; } }
        public int OutUnSeen { get { return OutTotal - OutSeen; } }
        public int OutTotal { get; set; }
        public int InAccepted { get; set; }
        public int InRedirected { get; set; }
        private int AccRedIn { get { return InAccepted + InRedirected; } }
        public int InSeen { get { return _seenIn > 0 ? _seenIn : 0; } set { _seenIn = value - AccRedIn; } }
        public int InTotal { get { return InAccepted + InRedirected + InSeen; } }
    }
}
