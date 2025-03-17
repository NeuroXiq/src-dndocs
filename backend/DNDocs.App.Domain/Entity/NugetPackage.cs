using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Entity
{
    public class NugetPackage : EntityBase
    {
        public string Title { get; set; }
        public string IdentityVersion { get; set; }
        public string IdentityId { get; set; }
        public DateTimeOffset? PublishedDate { get; set; }
        public string ProjectUrl { get; set; }
        public string PackageDetailsUrl { get; set; }
        public bool IsListed { get; set; }

        public NugetPackage() { }

        public NugetPackage(
            string title,
            string identityId,
            string identityVersion,
            DateTimeOffset? publishedDate,
            string projectUrl,
            string packageDetailsUrl,
            bool isListed)
        {
            Title = title;
            IdentityVersion = identityVersion;
            IdentityId = identityId;
            PublishedDate = publishedDate;
            ProjectUrl = projectUrl;
            PackageDetailsUrl = packageDetailsUrl;
            IsListed = isListed;
        }

        public string ToStringIdentityIdVer()
        {
            return $"{IdentityId} {IdentityVersion}";
        }
    }
}
