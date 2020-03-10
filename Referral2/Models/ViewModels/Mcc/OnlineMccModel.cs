using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Mcc
{
    public partial class OnlineMccModel
    {
        public int FacilityId { get;set; }
        public string FacilityName { get; set; }
        public bool Online { get; set; }
    }
}
