using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Exceptions;
using DNDocs.Domain.Repository;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Shared.Auth;
using System.Security.Claims;

namespace DNDocs.Infrastructure.Utils
{
    internal class CurrentUserImpl : ICurrentUser
    {
        private IAppUnitOfWork appUow;
        private IScopeContext bridge;
        private IRepository<User> userRepo;
        private ClaimsPrincipal _currentClaimsPrincipal;

        private ClaimsPrincipal currentClaimsPrincipal
        {
            get
            {
                if (_currentClaimsPrincipal == null)
                    _currentClaimsPrincipal = bridge.GetCurrentUser();
                return _currentClaimsPrincipal;
            }
        }

        public CurrentUserImpl(
            IScopeContext bridge,
            IAppUnitOfWork appUow)
        {
            this.appUow = appUow;
            this.bridge = bridge;
            this.userRepo = appUow.GetSimpleRepository<User>();
        }

        private IEnumerable<Claim> claims
        {
            get
            {
                if (!IsAuthenticated)
                {
                    throw new UnauthorizedException("Not authenticated");
                }

                return this.currentClaimsPrincipal.Claims;
            }
        }

        public User User { get { return this.userRepo.GetByIdChecked(UserIdAuthorized); } }

        public bool IsAuthenticated
        {
            get
            {
                return currentClaimsPrincipal != null &&
                    currentClaimsPrincipal.Claims.Count() > 0 &&
                    currentClaimsPrincipal.Claims.Any(c => c.Type == RobiniaClaims.UserId);
            }
        }

        public int UserIdAuthorized { get { Authorize();  return int.Parse(this.claims.First(t => t.Type == RobiniaClaims.UserId).Value); } }
        public bool IsAdmin => this.claims.FirstOrDefault(t => t.Type == RobiniaClaims.IsAdmin)?.Value == "true";

        public void AuthenticateAsUser(string fromUserLogin = null, int? fromUserId = null)
        {
            User user;
            if (!string.IsNullOrWhiteSpace(fromUserLogin))
            {
                user = appUow.GetSimpleRepository<User>().Query().FirstOrDefault(t => t.Login == fromUserLogin);
                if (user == null) throw new EntityNotFoundException<User>($"Tried to find user by login: '{fromUserLogin}'");
            }
            else if (fromUserId.HasValue)
            {
                user = appUow.GetSimpleRepository<User>().GetByIdChecked(fromUserId.Value);
            }
            else throw new RobiniaException("all parameters are null, specify one parameter to get user from db");

            var claims = new List<Claim>();
            bool isAdmin = user.Login == User.AdministratorUserLogin || user.Login == User.RobiniaAppServiceUserLogin;
            
            if (isAdmin) claims.Add(new Claim(RobiniaClaims.IsAdmin, "true"));
            
            claims.Add(new Claim(RobiniaClaims.UserId, user.Id.ToString()));

            var ci = new ClaimsIdentity(claims);
            _currentClaimsPrincipal = new ClaimsPrincipal(ci);
        }

        public void AuthorizationAdmin()
        {
            if (!IsAdmin) throw new UnauthorizedAccessException("not admin");
        }

        public void AuthorizationProjectManage(int projectId)
        {
            int userid = UserIdAuthorized;
            if (IsAdmin) return;

            appUow.GetSimpleRepository<Project>().GetByIdChecked(projectId);

            bool isowner = appUow.GetSimpleRepository<RefUserProject>()
                .Query()
                .Where(t => t.ProjectId == projectId && t.UserId == userid)
                .Any();

            if (!isowner) throw new ForbiddenException("Not owner of current project");
        }

        public User GetServiceUser()
        {
            return appUow.GetSimpleRepository<User>().Query().Where(t => t.Login == User.RobiniaAppServiceUserLogin).Single();
        }

        public void Authorize()
        {
            if (!claims.Any(t => t.Type == RobiniaClaims.UserId))
            {
                throw new UnauthorizedException("Not authenticated");
            }
        }

        public void Forbidden(bool condition, string msg = "No access to resource")
        {
            if (IsAdmin) return;
            if (condition) throw new ForbiddenException(msg);
        }
    }
}
