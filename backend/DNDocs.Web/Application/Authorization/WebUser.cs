using DNDocs.Domain.ValueTypes;
using DNDocs.Shared.Auth;
using System.Security.Claims;

namespace DNDocs.Web.Application.Authorization
{
    public interface IWebUser
    {
        public UserDto GetCurrentUser();
        public bool IsAdmin();
        bool IsAuthenticated();
    }

    public class WebUser : IWebUser
    {
        private readonly ClaimsPrincipal _userFromHttpContext;

        private ClaimsPrincipal UserFromHttpContext
        {
            get
            {
                if (_userFromHttpContext == null)
                {
                    throw new InvalidOperationException("No claimsprincipal. User not Authenticated cannot user service");
                }

                return _userFromHttpContext;
            }
        }

        public WebUser(IHttpContextAccessor httpContextAccessor)
        {
            _userFromHttpContext = httpContextAccessor.HttpContext.User;
        }

        public bool IsAuthenticated() { return this._userFromHttpContext != null && GetClaim(RobiniaClaims.UserId) != null; }

        public UserDto GetCurrentUser()
        {
            throw new NotImplementedException();
        }

        public bool IsAdmin()
        {
            return GetClaim(RobiniaClaims.IsAdmin) == "true";
        }

        private string GetClaim(string type)
        {
            return this.UserFromHttpContext.Claims.FirstOrDefault(t => t.Type == type)?.Value;
        }
    }
}
