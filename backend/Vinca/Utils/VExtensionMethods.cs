using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vinca.Http;

namespace Vinca.Utils
{
    public static class VExtensionMethods
    {
        public static void UseVHttpExceptions(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<VHttpExceptionMiddleware>();
        }
    }
}
