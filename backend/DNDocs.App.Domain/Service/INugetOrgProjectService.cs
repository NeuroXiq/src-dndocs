using DNDocs.App.Domain.Entity;
using DNDocs.Docs.Api.Client;
using DNDocs.Domain.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.App.Domain.Service
{
    public interface INugetOrgProjectService
    {
        Task DeleteAsync(int id);
    }

    public class NugetOrgProjectService : INugetOrgProjectService
    {
        private IAppUnitOfWork uow;
        private IDDocsApiClient ddocsApiClient;
        
        public NugetOrgProjectService(IDDocsApiClient ddocsApiClient, IAppUnitOfWork uow)
        {
            this.uow = uow;
            this.ddocsApiClient = ddocsApiClient;
        }

        public async Task DeleteAsync(int id)
        {
            await uow.GetSimpleRepository<NugetOrgProject>().DeleteAsync(id);
            await ddocsApiClient.Management_TryDeleteProjectAsync(id, Docs.Api.Management.ProjectType.NugetOrg);
        }
    }
}
