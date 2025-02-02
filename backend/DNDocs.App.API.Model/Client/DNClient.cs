using DNDocs.Api.DTO;
using DNDocs.Api.Model.Integration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DNDocs.Api.Client
{
    public interface IDNClient
    {
        Task<CommandResultDto> Integration_DJobBuildCompleted(DJobBuildCompletedModel model);
        Task<CommandResultDto> Integration_DJobRegisterService(DJobRegisterServiceModel model);
    }

    internal class DNClient : IDNClient
    {
        private HttpClient client;

        public DNClient(DNClientOptions options)
        {
            // for now ignore tls certs
            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            handler.ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
            
            client = new HttpClient(handler);
            
            client.BaseAddress = new Uri(options.ServerUrl);
            client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
        }

        public async Task<CommandResultDto> Integration_DJobBuildCompleted(DJobBuildCompletedModel model)
        {
            return await HandleResponseAsync(client.PostAsJsonAsync(Urls.Integration_DJobBuildCompleted, model, default));
        }

        public async Task<CommandResultDto> Integration_DJobRegisterService(DJobRegisterServiceModel model)
        {
            return await HandleResponseAsync(client.PostAsJsonAsync(Urls.Integration_DJobRegisterService, model, default));
        }

        async Task<CommandResultDto> HandleResponseAsync(Task<HttpResponseMessage> httpResponseTask)
        {
            var response = await httpResponseTask;
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<CommandResultDto>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }

    public class DNClientOptions
    {
        public string ServerUrl { get; set; }
        public string ApiKey { get; set; }
    }
}
