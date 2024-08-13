using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Http.Logs
{
    public interface IVHttpLogService
    {
        bool ShouldSaveLog(HttpContext context);
        void SaveLog(VHttpLog log);
        IEnumerable<VHttpLog> DequeueAll();
    }

    internal class VHttpLogService : IVHttpLogService
    {
        private VHttpLogOptions options;
        private ConcurrentQueue<VHttpLog> queue;

        public VHttpLogService(IOptions<VHttpLogOptions> options)
        {
            this.options = options.Value;
            queue = new ConcurrentQueue<VHttpLog>();
        }

        public IEnumerable<VHttpLog> DequeueAll()
        {
            if (queue.IsEmpty) return Enumerable.Empty<VHttpLog>();
            var max = queue.Count;
            List<VHttpLog> logs = new List<VHttpLog>();

            for (int i = 0; i < max && queue.TryDequeue(out var current); i++) logs.Add(current);

            return logs;
        }

        public void SaveLog(VHttpLog log)
        {
            queue.Enqueue(log);
        }

        public bool ShouldSaveLog(HttpContext context)
        {
            return options.ShouldSaveLog == null ? true : options.ShouldSaveLog(context);
        }
    }

    public class VHttpLogOptions
    {
        public int MaxQueueSize { get; set; }
        public Func<HttpContext, bool> ShouldSaveLog { get; set; }
    }
}
