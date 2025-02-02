using DNDocs.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.App.Domain.Entity
{
    public class BuildLog : EntityBase
    {
        public DateTime StartOn { get; set; }
        public DateTime CompletedOn { get; set; }
        public bool Success { get; set; }
        public string Logs { get; set; }
        public int NugetOrgProjectId { get; set; }
        public int DJobBuildId { get; set; }
    }
}
