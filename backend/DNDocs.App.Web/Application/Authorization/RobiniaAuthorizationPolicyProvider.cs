using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace DNDocs.Web.Application.Authorization
{
    public class RobiniaAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        public RobiniaAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            // copy & paste from microsoft docs
            BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }

        public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            // copy & paste from microsoft docs
            return BackupPolicyProvider.GetDefaultPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            // copy & paste from microsoft docs
            return BackupPolicyProvider.GetFallbackPolicyAsync();
        }

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(AuthorizationAttribute.Prefix))
            {
                var name = policyName.Substring(AuthorizationAttribute.Prefix.Length);

                var policy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme);
                policy.AddRequirements(new RobiniaAuthorizationRequirement(name));

                return Task.FromResult<AuthorizationPolicy>(policy.Build());
            }

            return Task.FromResult<AuthorizationPolicy>(null);
        }
    }
}
