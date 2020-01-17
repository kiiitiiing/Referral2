using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public partial class DashboardViewModel
    {
        public DashboardViewModel(int[] accepted, int[] redirected)
        {
            Accepted = accepted;
            Redirected = redirected;
            Max = accepted.Max() > redirected.Max() ? accepted.Max() : redirected.Max();
        }
        public int[] Accepted { get; set; }
        public int[] Redirected { get; set; }
        public int Max { get; set; }
    }
}
