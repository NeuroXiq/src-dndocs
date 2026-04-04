using Microsoft.VisualBasic;
using System.Reflection;

namespace Vinca.Utils
{
    public class SystemHealthApiResponse
    {
        public bool Online { get; set; }

        public string AppName { get; set; }

        public string AppAssemblyInformationalVersion { get; private set; }

        public Version AppAssemblyVersion { get; private set; }

        public DateTimeOffset Timestamp { get; set; }

        public static SystemHealthApiResponse Create(Assembly assembly)
        {
            var result = new SystemHealthApiResponse();

            result.Online = true;
            result.AppName = assembly.FullName;
            result.AppAssemblyInformationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            result.AppAssemblyVersion = assembly.GetName()?.Version;

            return result;
        }
    }
}
