namespace DNDocs.Docs.Web.Model
{
    public enum ProjectType : byte
    {
        Singleton = 1,
        Version = 2,
        Nuget = 3
    }

    public class Project
    {
        public long Id { get; set; }
        public long DnProjectId { get; set; }
        public string Metadata { get; set; }
        public string UrlPrefix { get; set; }
        public string ProjectVersion { get; set; }

        public string NugetPackageName { get; set; }
        public string NugetPackageVersion { get; set; }

        public ProjectType ProjectType { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public Project() { }

        //public Project(long projectId,
        //    string dndocsAppVersion,
        //    string urlPrefix,
        //    string version,
        //    string nPkgName,
        //    string nPkgVer,
        //    ProjectType type)
        //{
            
        //}
    }
}
