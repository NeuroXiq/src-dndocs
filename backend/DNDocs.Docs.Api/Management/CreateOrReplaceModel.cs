using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Docs.Api.Management
{
    public enum ProjectType
    {
        Singleton = 1,
        Version = 2,
        Nuget = 3,
    }

    public class CreateProjectModel
    {
        public int ProjectId { get; set; }
        public string Metadata { get; set; }
        public string ProjectName { get; set; }
        public string UrlPrefix { get; set; }
        public int ProjectType { get; set; }

        public string PVVersionTag { get; set; }

        public string NPackageName { get; set; }
        public string NPackageVersion { get; set; }
        public IFormFile SiteZip { get; set; }
    }
}
