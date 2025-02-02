using DNDocs.Api.DTO.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.MyAccount
{
    public class WaitingForBgJobModel
    {
        public virtual int BgJobId { get; set; }
        public virtual WaitingForBgJobType Type { get; set; }
    }
}
