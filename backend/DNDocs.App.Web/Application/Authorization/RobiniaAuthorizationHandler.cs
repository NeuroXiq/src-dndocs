using Microsoft.AspNetCore.Authorization;
using DNDocs.Domain.Utils;

namespace DNDocs.Web.Application.Authorization
{
    public class RobiniaAuthorizationHandler : AuthorizationHandler<RobiniaAuthorizationRequirement>
    {
        private IAppManager appManager;
        private IWebUser webUser;

        public RobiniaAuthorizationHandler(IAppManager appManager,
            IWebUser webUser)
        {
            this.appManager = appManager;
            this.webUser = webUser;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            RobiniaAuthorizationRequirement requirement)
        {
            if (!this.webUser.IsAuthenticated()) return Task.CompletedTask;

            var policyData = requirement.Requirement;

            if (this.webUser.IsAdmin())
            {
                context.Succeed(requirement);
            }

            switch (policyData)
            {
                case PolicyData.Administrator: break;
            }

            _return:
            return Task.CompletedTask;
        }
    }
}
