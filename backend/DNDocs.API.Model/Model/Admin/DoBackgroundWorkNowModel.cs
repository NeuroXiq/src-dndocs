using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.Admin
{
    public class DoBackgroundWorkNowModel
    {
        public bool ForceAll { get; set; }
        public bool ForceQueuedItems { get; set; }
        public bool ForceGenerateSitemap { get; set; }
        public bool ForceCheckHttpStatusForProjects { get; set; }
    }
}
