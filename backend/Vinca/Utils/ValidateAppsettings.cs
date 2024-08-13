using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Utils
{
    public class ValidateAppsettings
    {
        public static void Validate(Type obj, ConfigurationManager config)
        {
            ValidateElement(obj, $"{obj.Name}", config);
        }

        private static void ValidateElement(Type obj, string cpath, ConfigurationManager config)
        {
            var allProps = obj.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
            var simpleProps = allProps.Where(t => t.PropertyType.IsValueType || t.PropertyType == typeof(string)).ToArray();
            var complexProps = allProps.Where(t => !simpleProps.Any(taken => taken == t)).ToArray();

            foreach (var sp in simpleProps)
            {
                var configPath = $"{cpath}:{sp.Name}";
                var value = config.GetValue(sp.PropertyType, configPath);
                
                if (value == null)
                {
                    throw new ArgumentException($"Startup exception - appsettings: No value in appsetting for: {configPath}");
                }
            }

            foreach (var cp in complexProps)
            {
                ValidateElement(cp.PropertyType, $"{cpath}:{cp.Name}", config);
            }
        }
    }
}
