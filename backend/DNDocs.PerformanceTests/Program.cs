
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Program
{
    class Stats
    {
        public int TaskCount;
        public int RequestCount;
        public long BytesReceived;
        public long RequestDuration;
        public DateTime StartOn;

        public int ErrorsCount;
    }

    class PerformanceTestConfig
    {
        public int TaskCount;
        public int DelayRequestMs;
        public int TotalMaxRequestCount;
        public List<string> RootHtmlPageUrls;
    }

    class Program
    {
        static Stats stats;

        public static void Main()
        {
            stats = new Stats();
            stats.RequestCount = 1;
            PerformanceTestConfig config = new PerformanceTestConfig()
            {
                TaskCount = 300,
                DelayRequestMs = 200,
                TotalMaxRequestCount = 200000,
                RootHtmlPageUrls = new List<string>()
                {
                    "https://localhost:7088/system/projects/27",
                    "https://localhost:7088/system/projects/26",
                    "https://localhost:7088/system/projects/25"
                }
            };

            Timer refreshUiTimer = new Timer(RefreshUI, null, 1, 1000);

            stats.StartOn = DateTime.UtcNow;
            Start(config, stats);

            Console.ReadLine();
        }

        static void RefreshUI(object _)
        {
            var ellapsed = (DateTime.UtcNow - stats.StartOn).TotalSeconds;
            double bytesReceivedMB = (double)stats.BytesReceived / 1000000;

            Console.Clear();
            Console.WriteLine("{0, -20} {1}", "TaskCount", stats.TaskCount);
            Console.WriteLine("{0, -20} {1}", "RequestCount", stats.RequestCount);
            Console.WriteLine("{0, -20} {1:n3}MB", "BytesReceived", bytesReceivedMB);
            Console.WriteLine("{0, -20} {1:n}ms", "RequestDuration", stats.RequestDuration);
            Console.WriteLine("{0, -20} {1}s", "Ellapsed", ellapsed);
            Console.WriteLine("{0, -20} {1}", "ErrorsCount", stats.ErrorsCount);
            Console.WriteLine("----");
            
            Console.WriteLine("{0, -20} {1}", "req / sec", (double)stats.RequestCount / ellapsed);
            Console.WriteLine("{0, -20} {1:n}", "MB / s", bytesReceivedMB / ellapsed);
            Console.WriteLine("{0, -20} {1:n}", "dur / req ms", stats.RequestDuration / stats.RequestCount);
        }

        static void Start(PerformanceTestConfig config, Stats stats)
        {
            for (int i = 0; i < config.TaskCount; i++)
            {
                // Thread t = new Thread(() => { RunTaskDoPerformanceWork(config, stats).Wait(); });
                // t.Start();
                // Task.Factory.StartNew(() => RunTaskDoPerformanceWork(config, stats), TaskCreationOptions.LongRunning);
                Task.Run(() => RunTaskDoPerformanceWork(config, stats));
            }
        }

        private static async Task RunTaskDoPerformanceWork(PerformanceTestConfig config, Stats stats)
        {
            List<string> urls = config.RootHtmlPageUrls.ToList();
            List<string> nextUrls = new List<string>();
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("accept", "text/html");
            client.DefaultRequestHeaders.Add("accept-encoding", "gzip, deflate, br, zstd");
            Interlocked.Increment(ref stats.TaskCount);

            while (urls.Count > 0 && !(config.TotalMaxRequestCount < stats.RequestCount))
            {
                foreach (var url in urls)
                {
                    if (config.TotalMaxRequestCount < stats.RequestCount) break;

                    try
                    {
                        Interlocked.Increment(ref stats.RequestCount);
                        var sw = Stopwatch.StartNew();

                        var requestResult = await client.GetAsync(url);

                        var content = await requestResult.Content.ReadAsStringAsync();

                        nextUrls.AddRange(FindHtmlUrls(content));

                        Interlocked.Add(ref stats.RequestDuration, sw.ElapsedMilliseconds);
                        Interlocked.Add(ref stats.BytesReceived, content.Length);
                        
                        if (config.DelayRequestMs > 0) await Task.Delay(config.DelayRequestMs);
                    }
                    catch (Exception)
                    {
                        Interlocked.Increment(ref stats.ErrorsCount);
                    }
                }

                urls = nextUrls;
                nextUrls = new List<string>();
            }
        }

        private static List<string> FindHtmlUrls(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return new List<string>();

            var regex = new Regex("<a [^>]*href=(?:'(?<href>.*?)')|(?:\"(?<href>.*?)\")", RegexOptions.IgnoreCase);
            var urls = regex.Matches(content).OfType<Match>().Select(m => m.Groups["href"].Value).ToList();

            return urls;
        }
    }
}