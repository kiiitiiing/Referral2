
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Referral2.Models.ViewModels
{
    public partial class ChatsModel
    {
        [Required]
        public string Code { get; set; }
        [Required]
        public int Sender { get; set; }
        [Required]
        public string Message { get; set; }
        public List<Feedback> Chats { get; set; }

    }
}
