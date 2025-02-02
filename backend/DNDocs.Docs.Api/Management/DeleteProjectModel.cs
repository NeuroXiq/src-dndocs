namespace DNDocs.Docs.Api.Management
{
    public class DeleteProjectModel
    {
        public int ProjectId { get; set; }
        public ProjectType Type { get; set; }

        public DeleteProjectModel() { }

        public DeleteProjectModel(int projectId, ProjectType type)
        {
            ProjectId = projectId;
            Type = type;
        }
    }
}
