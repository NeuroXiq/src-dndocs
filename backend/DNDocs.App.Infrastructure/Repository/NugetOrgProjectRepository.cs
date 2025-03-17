using Microsoft.EntityFrameworkCore;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Repository;
using System.Diagnostics;
using DNDocs.Domain.Entity;
using DNDocs.App.Domain.Entity;

namespace DNDocs.Infrastructure.Repository
{
    public class NugetOrgProjectRepository: BaseRepository<NugetOrgProject>, INugetOrgProjectRepository
    {
        public NugetOrgProjectRepository(DbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
