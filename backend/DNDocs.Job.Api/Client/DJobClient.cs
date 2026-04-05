using DNDocs.Job.Api.Management;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using Vinca.Http;

namespace DNDocs.Job.Api.Client
{
    public interface IDNDocsJobApiClient
    {
        public Task PingAsync();
        public Task<HttpResponseMessage> BuildNugetOrgProject(BuildNugetOrgProjectModel model);
    }

    class asdf : HttpClientHandler
    {
        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.Send(request, cancellationToken);
        }


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }

    public class DNDocsJobApiClientOptions
    {
        public string ApiKey { get; set; }
        public string ServerUrl { get; set; }
    }

    public class DNDocsJobApiClient : IDNDocsJobApiClient
    {
        private ILogger<DNDocsJobApiClient> logger;
        private DNDocsJobApiClientOptions options;
        private HttpClient client;

        public DNDocsJobApiClient(IOptions<DNDocsJobApiClientOptions> options, ILogger<DNDocsJobApiClient> logger)
        {
            // for now ignore tls certs
            this.logger = logger;
            this.options = options.Value;
            var handler = new HttpClientHandlerLogger(logger);

            client = new HttpClient(handler);
            client.BaseAddress = new Uri(this.options.ServerUrl);
            client.DefaultRequestHeaders.Add("x-api-key", this.options.ApiKey);
        }

        public async Task PingAsync()
        {
            var result = await client.GetAsync(Urls.Ping);
            
            result.EnsureSuccessStatusCode();
        }

        public async Task<HttpResponseMessage> BuildNugetOrgProject(BuildNugetOrgProjectModel model)
        {
            var result = await client.PostAsJsonAsync(Urls.BuildProject, model);
            result.EnsureSuccessStatusCode();
            return result;
        }
    }
}
