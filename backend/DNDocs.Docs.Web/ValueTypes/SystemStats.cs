using DNDocs.Docs.Web.Model;

namespace DNDocs.Docs.Web.ValueTypes
{
    public class SystemStats
    {
        public long SiteItemCount { get; set; }
        public long SharedSiteItemCount { get; set; }
        public long HttpLogCount { get; set; }
        public long AppLogCount { get; set; }
        public long ProjectCount { get; set; }
        public long SiteItemCountUsingShared { get; set; }
    }
}
