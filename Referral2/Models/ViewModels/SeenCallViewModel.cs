using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class SeenCallViewModel
    {
        public string MdName { get; set; }
        public DateTime SeenDate { get; set; }
        public string MdContact { get; set; }
    }
}
