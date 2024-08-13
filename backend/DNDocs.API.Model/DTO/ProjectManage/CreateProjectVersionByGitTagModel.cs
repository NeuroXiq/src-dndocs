namespace DNDocs.Api.DTO.ProjectManage
{
    public class CreateProjectVersionByGitTagModel
    {
        public int ProjectVersioningId { get; set; }
        public string GitTagName { get; set; }

        public CreateProjectVersionByGitTagModel() { }
        public CreateProjectVersionByGitTagModel(int versioningId, string gitTagName)
        {
            ProjectVersioningId = versioningId;
            GitTagName = gitTagName;
        }

    }
}
