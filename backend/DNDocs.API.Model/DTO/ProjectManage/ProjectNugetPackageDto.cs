namespace DNDocs.Api.DTO.ProjectManage
{
    public class NugetPackageDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string IdentityVersion { get; set; }
        public string IdentityId { get; set; }
        public DateTimeOffset? PublishedDate { get; set; }
        public string ProjectUrl { get; set; }
        public string PackageDetailsUrl { get; set; }
        public bool IsListed { get; set; }
    }
}
