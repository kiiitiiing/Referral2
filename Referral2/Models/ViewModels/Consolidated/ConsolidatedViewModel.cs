
using System.Collections.Generic;

namespace Referral2.Models.ViewModels.Consolidated
{
    public partial class ConsolidatedViewModel
    {
        public int FacilitId { get; set; }
        public string FacilityLogo { get; set; }
        public string FacilityName { get; set; }
        //incoming
        public int InIncoming { get; set; }
        public int InAccepted { get; set; }
        public int InViewed { get; set; }
        public double InAcceptance { get; set; }
        public double InArrival { get; set; }
        public int InHorizontal { get; set; }
        public int InVertical { get; set; }
        public List<ListItem> InReferringFacilities { get; set; }
        public List<ListItem> InReferringDoctors { get; set; }
        public List<ListItem> InDiagnosis { get; set; }
        public List<ListItem> InReason { get; set; }
        public List<ListItem> InTransportation { get; set; }
        public List<ListItem> InDepartment { get; set; }
        public List<ListItem> InRemarks { get; set; }
        //outgoing
        public int OutOutgoing { get; set; }
        public int OutAccepted { get; set; }
        public int OutViewed { get; set; }
        public int OutRedirected { get; set; }
        public int OutArchived { get; set; }
        public int OutViewedAcceptance { get; set; }
        public int OutViewedRedirection { get; set; }
        public int OutAcceptance { get; set; }
        public int OutRedirection { get; set; }
        public int OutTransport { get; set; }
        public int OutHorizontal { get; set; }
        public int OutVertical { get; set; }
        public List<ListItem> OutReferredFacility { get; set; }
        public List<ListItem> OutReferredDoctor { get; set; }
        public List<ListItem> OutDiagnosis { get; set; }
        public List<ListItem> OutReason { get; set; }
        public List<ListItem> OutTransportation { get; set; }
        public List<ListItem> OutDepartment { get; set; }
        public List<ListItem> OutRemarks { get; set; }
    }
}
