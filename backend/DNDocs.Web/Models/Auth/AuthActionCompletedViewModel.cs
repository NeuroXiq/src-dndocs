namespace DNDocs.Web.Models.Auth
{
    public enum AuthActionType
    {
        Login,
        AdminLogin,
        Logout
    }

    public class AuthActionCompletedViewModel
    {
        public AuthActionType ActionType { get; set; }
        public bool Success { get; set; }
        public string RedirectUrl { get; set; }

        public AuthActionCompletedViewModel(
            AuthActionType type,
            bool success,
            string redirectUrl)
        {
            ActionType = type;
            Success = success;
            RedirectUrl = redirectUrl;
        }
    }
}
