using DNDocs.Domain.Entity.App;

namespace DNDocs.Domain.Utils
{
    public interface ICurrentUser
    {
        public int UserIdAuthorized { get; }
        bool IsAdmin { get; }
        bool IsAuthenticated { get; }
        User User { get; }

        void AuthorizationProjectManage(int projectId);
        void AuthorizationAdmin();
        User GetServiceUser();
        void Authorize();
        void Forbidden(bool condition, string msg = "");
        void AuthenticateAsUser(string fromUserLogin = null, int? fromUserId = null);
    }
}
