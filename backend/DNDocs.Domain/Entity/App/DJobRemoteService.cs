using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Domain.Entity.App
{
    public class DJobRemoteService : Entity
    {
        public string InstanceName { get; set; }
        public string ServerIpAddress { get; set; }
        public int ServerPort { get; set; }
        public bool Alive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }

        public DJobRemoteService(string instanceName, string serverIpAddress, int serverPort)
        {
            InstanceName = instanceName;
            ServerIpAddress = serverIpAddress;
            ServerPort = serverPort;
            CreatedOn = DateTime.UtcNow;
            UpdatedOn = DateTime.UtcNow;
        }
    }
}
