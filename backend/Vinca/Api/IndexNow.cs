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
        private VIndexNowOptions options;
        private ILogger<IndexNowApi> logger;
        private HttpClient httpClient;

        public IndexNowApi(
            IOptions<VIndexNowOptions> indexNowOptions,
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

    public class VIndexNowOptions
    {
        public string Host { get; set; }
        public string Key { get; set; }
        public string KeyLocation { get; set; }
        public string SubmitUrl { get; set; }
    }

    public static class IndexNowExtensions
    {
        public static void AddVIndexNowApi(this IServiceCollection services,
            Action<VIndexNowOptions> configure = null)
        {
            var optionsBuilder = services.AddOptions<VIndexNowOptions>();

            if (configure != null) optionsBuilder.Configure(configure);

            optionsBuilder
                .Validate(o => !string.IsNullOrWhiteSpace(o.Host), "host null or empty")
                .Validate(o => !string.IsNullOrWhiteSpace(o.Key), "key null or empty")
                .Validate(o => !string.IsNullOrWhiteSpace(o.KeyLocation), "keyLocation null or empty")
                .Validate(o => !string.IsNullOrWhiteSpace(o.SubmitUrl), "submitUrl null or empty")
                .ValidateOnStart();

            services.AddSingleton<IIndexNowApi, IndexNowApi>();
        }
    }
}
