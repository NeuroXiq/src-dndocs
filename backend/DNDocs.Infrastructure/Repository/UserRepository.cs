using DNDocs.Domain.Entity.App;
using DNDocs.Domain.Repository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Infrastructure.Repository
{
    internal class UserRepository : BaseRepository<User>, IUserRepository
    {
        public UserRepository(DbContext dbcontext) : base(dbcontext)
        {
        }

        public async Task<User> GetByLoginAsync(string login)
        {
            return await dbset.Where(t => t.Login == login).FirstOrDefaultAsync();
        }
    }
}
