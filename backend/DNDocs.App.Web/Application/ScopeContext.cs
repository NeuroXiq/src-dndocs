using DNDocs.Infrastructure.Utils;
using System.Security.Claims;

namespace DNDocs.Web.Application
{
    public class ScopeContext : IScopeContext
    {
        public IHttpContextAccessor HttpContextAccessor { get; }
        public string TraceIdentifier { get { return HttpContextAccessor?.HttpContext?.TraceIdentifier; } }
        public int? BgJobId { get; set; }

        public ScopeContext(IHttpContextAccessor httpContextAccessor)
        {
            this.HttpContextAccessor = httpContextAccessor;
        }

        public ClaimsPrincipal GetCurrentUser()
        {
            return HttpContextAccessor.HttpContext.User;
        }
    }
}
