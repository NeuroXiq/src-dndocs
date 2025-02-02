using Microsoft.EntityFrameworkCore;
using DNDocs.Domain.Enums;
using DNDocs.Domain.Repository;
using System.Diagnostics;
using DNDocs.Domain.Entity;

namespace DNDocs.Infrastructure.Repository
{
    internal class BgJobRepository : BaseRepository<BgJob>, IBgJobRepository
    {
        public BgJobRepository(DbContext dbcontext) : base(dbcontext)
        {
        }
    }
}
