namespace DNDocs.Docs.Api.Management
{
    public class DeleteProjectModel
    {
        public int ProjectId { get; set; }

        public DeleteProjectModel() { }

        public DeleteProjectModel(int projectId)
        {
            ProjectId = projectId;
        }
    }
}
