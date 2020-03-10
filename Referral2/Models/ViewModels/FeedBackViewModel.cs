using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class FeedbackViewModel
    {
        public string Code { get; set; }
        public int MdId { get; set; }
        public string Facility { get; set; }
        public string MdName { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime TimeSent { get; set; }
    }
}
