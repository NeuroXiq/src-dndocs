using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Api;
using Vinca.Api.Nuget;

namespace Vinca.Utils
{
    public static class StringExtensions
    {
        public static string StringJoin<T>(this IEnumerable<T> enumerable, string separator, Func<T, string> propSelector)
        {
            if (enumerable == null || !enumerable.Any()) return string.Empty;

            return string.Join(separator, enumerable.Select(t => propSelector(t)));
        }

        public static string StringJoin<T>(this IEnumerable<T> enumerable, string separator)
        {
            return StringJoin(enumerable, separator, t => t?.ToString() ?? string.Empty);
        }

        public static string ToStringSql(this DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }

    public static class VRegisterExtensions
    {
        public static void AddVOSApi(this IServiceCollection services)
        {
            services.TryAddSingleton<IOSApi, OSApi>();
        }

        public static void AddVNugetRepositoryFacade(this IServiceCollection services)
        {
            AddVOSApi(services);
            services.AddScoped<INugetRepositoryFacade, NugetRepositoryFacade>();
        }
    }
}
