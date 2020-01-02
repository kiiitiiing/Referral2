using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels
{
    public class RemarksViewModel
    {
        [Required]
        public string Code { get; set; }

        [DataType(DataType.MultilineText)]
        public string Remarks { get; set; }
    }
}
