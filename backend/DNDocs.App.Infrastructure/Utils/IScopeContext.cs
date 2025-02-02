using System.Security.Claims;

namespace DNDocs.Infrastructure.Utils
{
    // how to get tenant from url from robinia.web?
    // solution for now
    public interface IScopeContext
    {
        string TraceIdentifier { get; }
        int? BgJobId { get; set; }
        public ClaimsPrincipal GetCurrentUser();
    }
}
