using Microsoft.EntityFrameworkCore;
using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Repository;
using System.Diagnostics;

namespace DNDocs.Infrastructure.Repository
{
    internal class BgJobRepository : BaseRepository<BgJob>, IBgJobRepository
    {
        public BgJobRepository(DbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
