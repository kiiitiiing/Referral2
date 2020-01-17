using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class SupportUsersViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Facility { get; set; }
        [DataType(DataType.PhoneNumber)]
        public string Contact { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
