
using DNDocs.Domain.Entity.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Repository
{
    public interface IUserRepository : IRepository<User>
    {
        public Task<User> GetByLoginAsync(string login);
    }
}
