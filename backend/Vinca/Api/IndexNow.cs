using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.Api
{
    public interface IIndexNowApi
    {
        Task SubmitUrls(string[] urls);
    }

    internal class IndexNowApi : IIndexNowApi
    {
        private IndexNowOptions options;
        private ILogger<IndexNowApi> logger;
        private HttpClient httpClient;

        public IndexNowApi(
            IOptions<IndexNowOptions> indexNowOptions,
            IHttpClientFactory httpClientFactory,
            ILogger<IndexNowApi> logger)
        {
            options = indexNowOptions.Value;
            this.logger = logger;
            httpClient = httpClientFactory.CreateClient();
        }

        public async Task SubmitUrls(string[] urls)
        {
            if (urls == null) throw new ArgumentNullException("urls");
            if (urls.Length == 0) throw new ArgumentOutOfRangeException("0 urls");

            var request = new IndexNowRequest
            {
                Host = options.Host,
                Key = options.Key,
                KeyLocation = options.KeyLocation,
                UrlList = urls
            };

            logger.LogInformation("starting to send POST request. Api url: {0} ApiKey: {1}, Urls count: {2}",
                options.SubmitUrl,
                options.Key.Length < 3 ? $"***" : $"{options.Key[0]}***{options.Key.Last()}",
                urls.Length);

            var sw = Stopwatch.StartNew();

            try
            {
                var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var result = await httpClient.PostAsync(options.SubmitUrl, content);

                logger.LogInformation("completed send POST request. Response status code: {0}, duration: {1}ms", result.StatusCode, sw.ElapsedMilliseconds);

                result.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                logger.LogError(e, "IndexNow request failed");
                throw;
            }
            
        }
    }

    internal class IndexNowRequest
    {
        public string Host { get; set; }
        public string Key { get; set; }
        public string KeyLocation { get; set; }
        public string[] UrlList { get; set; }
    }

    internal class IndexNowOptions
    {
        public string Host { get; set; }
        public string Key { get; set; }
        public string KeyLocation { get; set; }
        public string SubmitUrl { get; internal set; }
    }

    public static class IndexNowExtensions
    {
        public static void AddVIndexNowApi(this IServiceCollection services,
            string submitUrl,
            string host,
            string key,
            string keyLocation)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException("host null or empty");
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key null or empty");
            if (string.IsNullOrWhiteSpace(keyLocation)) throw new ArgumentException("keyLocation null or empty");
            if (string.IsNullOrWhiteSpace(submitUrl)) throw new ArgumentException("submitUrl null or empty");

            services.Configure<IndexNowOptions>(c =>
            {
                c.Host = host;
                c.Key = key;
                c.SubmitUrl = submitUrl;
                c.KeyLocation = keyLocation;
            });

            services.AddSingleton<IIndexNowApi, IndexNowApi>();
        }
    }
}
