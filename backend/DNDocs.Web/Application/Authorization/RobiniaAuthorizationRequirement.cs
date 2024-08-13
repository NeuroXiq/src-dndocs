using Microsoft.AspNetCore.Authorization;

namespace DNDocs.Web.Application.Authorization
{
    public class RobiniaAuthorizationRequirement : IAuthorizationRequirement
    {
        public string Requirement { get; set; }

        public RobiniaAuthorizationRequirement(string requirement)
        {
            Requirement = requirement;
        }
    }
}
