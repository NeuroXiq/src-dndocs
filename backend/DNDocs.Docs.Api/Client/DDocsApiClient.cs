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
                string pvVersionTag,
                string nPackageName,
                string nPackageVersion,
                int projectType,
                Stream zipStream
                );
    }

    public class DDocsApiClient : IDDocsApiClient
    {
        private DDocsApiClientOptions options;
        private HttpClient client;

        public DDocsApiClient(DDocsApiClientOptions ioptions)
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
            options = ioptions;
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

        public async Task<DDocsApiResult> Management_CreateProject(
            long projectId,
            string projectName,
            string metadata,
            string urlPrefix,
            string pvVersionTag,
            string nPackageName,
            string nPackageVersion,
            int projectType,
            Stream zipStream
            )
        {
            MultipartFormDataContent form = new MultipartFormDataContent();

            form.Add(new StringContent(projectId.ToString()), nameof(CreateProjectModel.ProjectId));
            form.Add(new StringContent(metadata ?? ""), nameof(CreateProjectModel.Metadata));
            form.Add(new StringContent(projectName), nameof(CreateProjectModel.ProjectName));
            form.Add(new StringContent(urlPrefix ?? ""), nameof(CreateProjectModel.UrlPrefix));
            form.Add(new StringContent(pvVersionTag ?? ""), nameof(CreateProjectModel.PVVersionTag));
            form.Add(new StringContent(projectType.ToString()), nameof(CreateProjectModel.ProjectType));
            form.Add(new StringContent(nPackageName ?? ""), nameof(CreateProjectModel.NPackageName));
            form.Add(new StringContent(nPackageVersion ?? ""), nameof(CreateProjectModel.NPackageVersion));

            //form.Add(new ByteArrayContent(siteZip, 0, siteZip.Length), "siteZip", "siteZip.zip");
            form.Add(new StreamContent(zipStream), "siteZip", "sitezip.zip");

            // var multipartContent = new MultipartFormDataContent();
            // 
            // // Add string fields
            // multipartContent.Add(new StringContent("your value here"), "field1");
            // multipartContent.Add(new StringContent("another value"), "field2");
            // 
            // // Add a file
            // var fileStream = new MemoryStream(new byte[] { 1, 2, 3 });
            // var streamContent = new StreamContent(fileStream);
            // streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            // 
            // multipartContent.Add(streamContent, "file", "ery");

            // HttpResponseMessage response = await httpClient.PostAsync(urls.Management_CreateOrReplaceProject, multipartContent);
            
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
    }


    public class DDocsApiResult
    {
        public HttpResponseMessage RawResponse { get; set; }
    }

    public class DDocsApiClientOptions
    {
        public string ApiKey { get; set; }
        public string ServerUrl { get; set; }

        public DDocsApiClientOptions() { }

        public DDocsApiClientOptions(string apiKey, string serverUrl)
        {
            ApiKey = apiKey;
            ServerUrl = serverUrl;
        }
    }
}
