using System;
namespace Referral2.Models.ViewModels.Support
{
    public partial class SupportChatViewModel
    {
        public int SupportId { get; set; }
        public string SupportFacilityName { get; set; }
        public string SupportName { get; set; }
        public string Message { get; set; }
        public DateTime MessageDate { get; set; }
    }
}
