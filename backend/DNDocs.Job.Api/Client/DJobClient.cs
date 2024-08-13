using DNDocs.Job.Api.Management;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Vinca.Http;

namespace DNDocs.Job.Api.Client
{
    public interface IDJobClient
    {
        public string ServerUrl { get; }
        public Task Ping();
        public Task<HttpResponseMessage> BuildProject(BuildProjectModel model);
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

    internal class DJobClient : IDJobClient
    {
        public string ServerUrl { get { return client.BaseAddress.ToString(); } }

        private ILogger<DJobClient> logger;
        private HttpClient client;

        public DJobClient(string serverUrl, string apiKey, ILogger<DJobClient> logger)
        {
            // for now ignore tls certs
            var handler = new HttpClientHandlerLogger(logger);
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };

            this.logger = logger;
            client = new HttpClient(handler);
            client.BaseAddress = new Uri(serverUrl);
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        public async Task Ping()
        {
            var result = await client.GetAsync(Urls.Ping);
            
            result.EnsureSuccessStatusCode();
        }

        public async Task<HttpResponseMessage> BuildProject(BuildProjectModel model)
        {
            var result = await client.PostAsJsonAsync(Urls.BuildProject, model);
            result.EnsureSuccessStatusCode();
            return result;
        }
    }

    public interface IDJobClientFactory
    {
        IDJobClient Create(string serverUrl, string apiKey);
        IDJobClient CreateFromIpPort(string ip, int port, string apiKey);
    }

    public class DJobClientFactory : IDJobClientFactory
    {
        private IServiceProvider serviceProvider;
        private ILoggerFactory loggerFactory;

        public DJobClientFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            this.serviceProvider = serviceProvider;
            this.loggerFactory = loggerFactory;
        }

        public IDJobClient Create(string serverUrl, string apiKey)
        {
            return new DJobClient(serverUrl, apiKey, loggerFactory.CreateLogger<DJobClient>());
        }

        public IDJobClient CreateFromIpPort(string ip, int port, string apiKey)
        {
            var url = new UriBuilder("https", ip, port).ToString();
            return Create(url, apiKey);
        }
    }
}
