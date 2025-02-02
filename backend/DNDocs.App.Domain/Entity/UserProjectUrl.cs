using DNDocs.Domain.Enums;

namespace DNDocs.Domain.Entity
{
    public class UserProjectUrl : Entity
    {
        public int UserId { get; set; }
        public string ProjectUrl { get; set; }
        public string ProjectName { get; set; }
        public ProjectWebsiteType ProjectWebsiteType { get; set; }
    }
}
