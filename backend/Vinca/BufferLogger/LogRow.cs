using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.BufferLogger
{
    public class LogRow
    {
        public string? Message { get; set; }
        public string? CategoryName { get; set; }
        public EventId EventId { get; set; }
        public LogLevel LogLevel { get; set; }
        // public string? Exception { get; set; }
        public DateTime Date { get; set; }
    }
}
