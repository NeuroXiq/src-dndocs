namespace DNDocs.Domain.Entity.App
{
    public class RefUserProject : Entity
    {
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public User User { get; set; }

        public RefUserProject() { }

        public RefUserProject(User user, Project project)
        {
            User = user;
            Project = project;
        }

        public RefUserProject(int userid, int projectid)
        {
            UserId = userid;
            ProjectId = projectid;
        }
    }
}
