using System.Collections.Generic;

namespace Referral2.Models.ViewModels
{
    public partial class PageListModel
    {
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public int TotalPages { get; set; }
        public string Action { get; set; }
        public int PageIndex { get; set; }
        public Dictionary<string,string> Parameters { get; set; }
    }
}
