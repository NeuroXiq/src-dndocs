using DNDocs.Shared.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace DNDocs.Shared.Utils
{
    public class Helpers
    {
        public static string ExceptionToStringForLogs(Exception e)
        {
            if (e == null) return string.Empty;

            StringBuilder b = new StringBuilder();
            b.AppendLine($"Type: {e.GetType().FullName}");
            b.AppendLine($"Message: {e.Message ?? ""}");
            b.AppendLine($"Source: {e.Source ?? ""}");
            b.AppendLine($"StackTrace:");
            b.AppendLine($"{e.StackTrace ?? ""}");

            if (e.InnerException != null)
            {
                b.AppendLine("InnerException:\r\n");
                b.Append(ExceptionToStringForLogs(e.InnerException));
            }

            return b.ToString();
        }
    }
}
