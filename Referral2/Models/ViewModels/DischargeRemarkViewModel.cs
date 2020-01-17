using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class DischargeRemarkViewModel
    {
        public string Code { get; set; }
        public DateTime DateDischarged { get; set; }
        public string Remarks { get; set; }
    }
}
