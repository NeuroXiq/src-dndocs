using DNDocs.Docs.Api.Management;
using DNDocs.Docs.Api.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DNDocs.Docs.Api.Client
{
    public interface IDDocsApiClient
    {
        public Task<string> Management_Ping(string pingToSend);

        public Task<DDocsApiResult> Management_CreateProject(
                long projectId,
                string projectName,
                string metadata,
                string urlPrefix,
                string nPackageName,
                string nPackageVersion,
                ProjectType projectType,
                Stream zipStream
                );

        Task<IList<SiteItemDto>> Management_GetSiteItemIdPaged(long startId, int count);

        Task Management_TryDeleteProjectAsync(int projectId, ProjectType type);
    }

    public class DDocsApiClient : IDDocsApiClient
    {
        private OptionsDDocsApiClient options;
        private HttpClient client;

        public DDocsApiClient(IOptions<OptionsDDocsApiClient> ioptions)
        {
            options = ioptions.Value;
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
            client.Timeout = TimeSpan.FromMinutes(2);
        }

        public async Task<string> Management_Ping(string pingToSend)
        {
            var result = await client.GetAsync($"/{DUrls.Management_Ping}/{pingToSend}");
            result.EnsureSuccessStatusCode();

            return await result.Content.ReadAsStringAsync();
        }

        public async Task Management_TryDeleteProjectAsync(int projectId, ProjectType type)
        {
            HandleResponse(await client.PostAsJsonAsync(DUrls.Management_TryDeleteProject, new DeleteProjectModel(projectId, type)));
        }

        private void HandleResponse(HttpResponseMessage response)
        {
            response.EnsureSuccessStatusCode();
        }

        public async Task<DDocsApiResult> Management_CreateProject(
            long projectId,
            string projectName,
            string metadata,
            string urlPrefix,
            string nPackageName,
            string nPackageVersion,
            ProjectType projectType,
            Stream zipStream
            )
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(projectId.ToString()), nameof(CreateProjectModel.ProjectId));
            form.Add(new StringContent(metadata ?? ""), nameof(CreateProjectModel.Metadata));
            form.Add(new StringContent(projectName), nameof(CreateProjectModel.ProjectName));
            form.Add(new StringContent(urlPrefix ?? ""), nameof(CreateProjectModel.UrlPrefix));
            form.Add(new StringContent(((int)projectType).ToString()), nameof(CreateProjectModel.ProjectType));
            form.Add(new StringContent(nPackageName ?? ""), nameof(CreateProjectModel.NPackageName));
            form.Add(new StringContent(nPackageVersion ?? ""), nameof(CreateProjectModel.NPackageVersion));
            form.Add(new StreamContent(zipStream), "siteZip", "sitezip.zip");
            
            HttpResponseMessage response = await client.PostAsync(DUrls.Management_CreateProject, form);
            var rawResponse = await response.Content.ReadAsStringAsync();

            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch
            {
                throw new Exception($"error during httprequest. Code:{response.StatusCode}\r\nResponse as string: \r\n{rawResponse}");
            }
            
            
            return MapResult(response);
        }

        public void Public_Ping()
        {

        }

        static DDocsApiResult MapResult(HttpResponseMessage response)
        {
            return new DDocsApiResult { RawResponse = response };        
        }

        public async Task<IList<SiteItemDto>> Management_GetSiteItemIdPaged(long startId, int count)
        {
            var httpResult = await client.GetAsync($"{DUrls.Management_GetSiteItemPaged}?startSiteItemId={startId}&count={count}");
            httpResult.EnsureSuccessStatusCode();
            var siteItems = await httpResult.Content.ReadFromJsonAsync<IList<SiteItemDto>>();

            return siteItems;
        }
    }

    public class DDocsApiResult
    {
        public HttpResponseMessage RawResponse { get; set; }
    }

    public class OptionsDDocsApiClient
    {
        public string ApiKey { get; set; }
        public string ServerUrl { get; set; }

        public OptionsDDocsApiClient() { }

        public OptionsDDocsApiClient(string apiKey, string serverUrl)
        {
            ApiKey = apiKey;
            ServerUrl = serverUrl;
        }
    }
}
