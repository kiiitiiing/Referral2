
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Referral2.Models.ViewModels.Admin
{
    public partial class DailyReferralViewModel
    {
        private int _seenTo;
        private int _seenFrom;
        public string Facility { get; set; }
        public int AcceptedTo { get; set; }
        public int RedirectedTo { get; set; }
        private int AccRedIn { get { return AcceptedTo + RedirectedTo; } }
        public int SeenTo { get { return _seenTo > 0 ? _seenTo : 0; } set { _seenTo = value - AccRedIn; } }
        public int IncomingTotal { get; set; }
        public int UnseenTo { get { return IncomingTotal - SeenTo; } }
        public int AcceptedFrom { get; set; }
        public int RedirectedFrom { get; set; }
        private int AccRedOut { get { return AcceptedFrom + RedirectedFrom; } }
        public int SeenFrom { get { return _seenFrom > 0 ? _seenFrom : 0; } set { _seenFrom = value - AccRedOut; } }
        public int OutgoingTotal { get { return AcceptedFrom + RedirectedFrom + SeenFrom; } }
    }
}
