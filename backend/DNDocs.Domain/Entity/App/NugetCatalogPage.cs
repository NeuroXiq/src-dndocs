using DNDocs.Domain.Entity.Shared;
using DNDocs.Domain.Enums;

namespace DNDocs.Domain.Entity.App
{
    public class NugetCatalogPage : Entity, ICreateUpdateTimestamp
    {
        public string NId { get; set; }

        public string CommitId { get; set; }

        public DateTime CommitTimeStamp { get; set; }

        public int Count { get; set; }

        public NugetCatalogPageState State { get; set; }

        public DateTime CreatedOn { get; set; }
        
        public DateTime LastModifiedOn { get; set; }

        public NugetCatalogPage() { }

        public NugetCatalogPage(string id, string commitId, DateTime commitTimeStamp, int count)
        {
            NId = id;
            CommitId = commitId;
            CommitTimeStamp = commitTimeStamp;
            Count = count;
            State = NugetCatalogPageState.ToProcess;
        }
    }
}
