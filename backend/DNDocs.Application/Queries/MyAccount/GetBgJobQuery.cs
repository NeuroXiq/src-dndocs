using DNDocs.Api.DTO.MyAccount;
using DNDocs.Application.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Application.Queries.MyAccount
{
    public class GetBgJobQuery : Query<BgJobViewModel>
    {
        public int ProjectId { get; set; }
    }
}
