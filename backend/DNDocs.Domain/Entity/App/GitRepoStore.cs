using DNDocs.Domain.Entity.Shared;
using DNDocs.Domain.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Entity.App
{
    public class GitRepoStore : Entity, ICreateUpdateTimestamp
    {
        public Guid UUID { get; set; }
        public string GitRepoUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModifiedOn { get; set; }
        public DateTime LastAccessOn { get; set; }

        public GitRepoStore() { }

        public GitRepoStore(string gitRepoUrl)
        {
            Validation.AppArgStringNotEmpty(gitRepoUrl, nameof(gitRepoUrl));

            UUID = Guid.NewGuid();
            GitRepoUrl = gitRepoUrl;
            CreatedOn = DateTime.UtcNow;
            LastModifiedOn = DateTime.UtcNow;
            LastAccessOn = DateTime.UtcNow;
        }
    }
}
