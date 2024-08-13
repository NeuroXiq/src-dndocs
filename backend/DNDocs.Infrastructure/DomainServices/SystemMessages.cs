using Microsoft.Extensions.DependencyInjection;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Service;
using DNDocs.Domain.UnitOfWork;
using DNDocs.Domain.Utils;
using DNDocs.Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Infrastructure.DomainServices
{
    internal class SystemMessages : ISystemMessages
    {
        private IScopeContext scopeContext;
        private IServiceProvider serviceProvider;

        public SystemMessages(IServiceProvider serviceProvider,
            IScopeContext scopeContext)
        {
            this.scopeContext = scopeContext;
            this.serviceProvider = serviceProvider;
        }

        public void Error(object obj, string title, string msg) => SaveMsg(obj, SystemMessageLevel.Error, title, msg);
        public void Info(object obj, string title, string msg) => SaveMsg(obj, SystemMessageLevel.Information, title, msg);
        public void Trace(object obj, string title, string msg) => SaveMsg(obj, SystemMessageLevel.Trace, title, msg);
        public void Warning(object obj, string title, string msg) => SaveMsg(obj, SystemMessageLevel.Warning, title, msg);
        public void Success(object obj, string title, string msg) => SaveMsg(obj, SystemMessageLevel.Success, title, msg);

        private void SaveMsg(object obj, SystemMessageLevel level, string title, string msg)
        {
            // save messages in other transaction
            // maybe there will be needed for e.g. debug or something
            // related entity must be committed in table in DB before message created

            var objType = obj?.GetType();
            var sysMsg = new SystemMessage(SystemMessageType.Other, level, title, msg, DateTime.UtcNow, scopeContext.BgJobId);

            if (objType == typeof(Project))
            {
                sysMsg.Type = SystemMessageType.Project;
                sysMsg.ProjectId = ((Project)obj).Id;
            }
            else if (objType == typeof(User))
            {
                sysMsg.Type = SystemMessageType.User;
                sysMsg.UserId = (obj as User).Id;
            }
            else if (objType == typeof(ProjectVersioning))
            {
                sysMsg.ProjectVersioningId = ((ProjectVersioning)obj).Id;
            }
            else if (objType != null)
            {
                throw new RobiniaException($"Unkonwn type for system message: '{objType.FullName}'");
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var uow = scope.ServiceProvider.GetRequiredService<IAppUnitOfWork>();
                uow.GetSimpleRepository<SystemMessage>().Create(sysMsg);
            }
        }
    }
}
