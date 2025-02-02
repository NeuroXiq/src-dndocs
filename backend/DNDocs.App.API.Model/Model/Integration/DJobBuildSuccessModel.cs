using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Api.Model.Integration
{
    public class DJobBuildCompletedModel
    {
        public long ProjectId { get; set; }
        public bool Success { get; set; }
        public string Exception { get; set; }
    }
}
