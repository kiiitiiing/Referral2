using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Support
{
    public partial class SupportManageViewModel
    {
        public int Id { get; set; }
        public string DoctorName { get; set; }
        public string DepartmentName { get; set; }
        public string Contact { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public string LastLogin { get; set; }
    }
}
