using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Cache
{
    public class CachedResourceInfo
    {
        public DateTime LastModifiedAt { get; set; }
        public string FileTag { get; set; }
    }
}
