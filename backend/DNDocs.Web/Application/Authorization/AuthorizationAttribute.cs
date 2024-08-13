using Microsoft.AspNetCore.Authorization;

namespace DNDocs.Web.Application.Authorization
{
    public class AuthorizationAttribute : AuthorizeAttribute
    {
        public const string Prefix = "Authorize$";

        public string PolicyData
        {
            get
            {
                return (base.Policy?.Substring(Prefix.Length) ?? "");
            }
            set
            {
                base.Policy = $"{Prefix}{value}";
            }
        }
    }
}
