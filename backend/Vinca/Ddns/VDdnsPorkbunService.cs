using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Vinca.Ddns
{
    public class VDdnsPorkbunService : IVDdnsService
    {
        private ILogger<VDdnsPorkbunService> logger;
        private OptionsVDdnsProkbunService options;
        private HttpClient httpClientPorkbun;

        public VDdnsPorkbunService(IOptions<OptionsVDdnsProkbunService> options,
            ILogger<VDdnsPorkbunService> logger)
        {
            this.logger = logger;
            this.options = options.Value;
            httpClientPorkbun = new HttpClient();
            httpClientPorkbun.BaseAddress = new Uri(this.options.ApiUrl);
        }

        public async Task UpdateDdnsAsync(CancellationToken token)
        {
            List<IPAddress> myIp = new List<IPAddress>();

            //await CallPorkbunApiDnsDeleteDomain("dndocs.com", "test1", "A");

            // await CallPorkbunApiDnsCreateDomain("dndocs.com", new PorkbunRequestDnsCreateRecord()
            // {
            //     Name = "test1",
            //     Ttl = "1234",
            //     Type = "A",
            //     Content = "127.0.0.2"
            // });

            try
            {
                myIp.Add(await CallPorkbunApiPingAsync());
            }
            catch (Exception e)
            {
                logger.LogError(e, "porkbun resolve ip failed");
            }

            var recordsOnPorkbun1 = await CallPorkbunRetrieveRecords(options.Domain, options.Subdomain, "A");
            var recordsOnPorkbun2 = await CallPorkbunRetrieveRecords(options.Domain, options.Subdomain, "AAAAA");

            var existingRecords = recordsOnPorkbun1.Records.Union(recordsOnPorkbun2.Records).ToList();

            if (recordsOnPorkbun1.Records.Any()) await CallPorkbunApiDnsDeleteDomain(options.Domain, options.Subdomain, "A");
            if (recordsOnPorkbun2.Records.Any()) await CallPorkbunApiDnsDeleteDomain(options.Domain, options.Subdomain, "AAAA");

            var ipv4 = myIp.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            var ipv6 = myIp.FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);

            if (ipv4 != null)
            {
                await CallPorkbunApiDnsCreateRecord(options.Domain, new PorkbunRequestDnsCreateRecord()
                {
                    Name = options.Subdomain,
                    Type = "A",
                    Content = ipv4.ToString()
                });
            }

            if (ipv6 != null)
            {
                await CallPorkbunApiDnsCreateRecord(options.Domain, new PorkbunRequestDnsCreateRecord()
                {
                    Name = options.Subdomain,
                    Type = "AAAA",
                    Content = ipv6.ToString()
                });
            }
        }

        private async Task<IPAddress> GetMyPublicIp()
        {
            return null;
        }

        async Task<PorkbunResponseRetireveRecords> CallPorkbunRetrieveRecords(string domain, string subdomain, string type)
        {
            return await CallPorkbunApiAsync<PorkbunRequestBase, PorkbunResponseRetireveRecords>($"/api/json/v3/dns/retrieveByNameType/{domain}/{type}/{subdomain}", new PorkbunRequestBase());
        }

        async Task<IPAddress> CallPorkbunApiPingAsync(CancellationToken token = default)
        {
            logger.LogTrace(nameof(CallPorkbunApiPingAsync));

            var pingResult = await CallPorkbunApiAsync<PorkbunRequestPing, PorkbunResponsePing>("/api/json/v3/ping", new PorkbunRequestPing());

            return IPAddress.Parse(pingResult.YourIp);
        }

        async Task CallPorkbunApiDnsDeleteDomain(string domain, string subdomain, string type)
        {
            await CallPorkbunApiAsync<PorkbunRequestBase, PorkbunResponse>($"/api/json/v3/dns/deleteByNameType/{domain}/{type}/{subdomain}", new PorkbunRequestBase());
        }

        async Task CallPorkbunApiDnsCreateRecord(string domain, PorkbunRequestDnsCreateRecord createRecordCommand)
        {
            await CallPorkbunApiAsync<PorkbunRequestDnsCreateRecord, PorkbunResponse>($"api/json/v3/dns/create/{domain}", createRecordCommand);
        }

        async Task<TResponse> CallPorkbunApiAsync<TRequest, TResponse>(string apiPath, TRequest value) where TRequest : PorkbunRequestBase
        {
            logger.LogTrace(nameof(CallPorkbunApiAsync));
            logger.LogTrace($"start calling porkbun api '{apiPath}'");

            SetApiKeys(value);

            var response = await httpClientPorkbun.PostAsJsonAsync(apiPath, value);

            if (!response.IsSuccessStatusCode)
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                logger.LogError($"porkbun request failed data: {response.StatusCode}\r\n{errorResult}");
            }

            response.EnsureSuccessStatusCode();

            logger.LogInformation("porkbun request completed with success status code");

            var result = await response.Content.ReadFromJsonAsync<TResponse>();

            logger.LogInformation("porkbun request read data success");

            return result;
        }

        void SetApiKeys(PorkbunRequestBase request)
        {
            request.ApiKey = this.options.ApiKey;
            request.SecretApiKey = this.options.SecretApiKey;
        }

        class PorkbunResponse
        {
            public string Status { get; set; }
            public string Message { get; set; }
        }

        class PorkbunRequestBase
        {
            // lowercase is important for porkbun
            [JsonPropertyName("apikey")]
            public string ApiKey { get; set; }

            [JsonPropertyName("secretapikey")]
            public string SecretApiKey { get; set; }
        }

        class PorkbunResponsePing
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("yourIp")]
            public string YourIp { get; set; }
        }

        class PorkbunRequestPing : PorkbunRequestBase
        {

        }

        class PorkbunRequestDnsCreateRecord : PorkbunRequestBase
        {
            /// <summary>
            /// The subdomain for the record being created, not including the domain itself.Leave blank to create a record on the root domain.Use* to create a wildcard record.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("name")]
            public string Name { get; set; }

            /// <summary>
            /// The type of record being created.Valid types are: A, MX, CNAME, ALIAS, TXT, NS, AAAA, SRV, TLSA, CAA, HTTPS, SVCB, SSHFP
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
            [JsonPropertyName("type")]
            public string Type { get; set; }


            /// <summary>
            /// The answer content for the record. Please see the DNS management popup from the domain management console for proper formatting of each record type.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
            [JsonPropertyName("content")]
            public string Content { get; set; }

            /// <summary>
            /// The time to live in seconds for the record. The minimum and the default is 600 seconds.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("ttl")]
            public string Ttl { get; set; }


            /// <summary>
            /// The priority of the record for those that support it.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("prio")]
            public string Prio { get; set; }


            /// <summary>
            /// Any notes that you'd like to set for the record.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("notes")]
            public string Notes { get; set; }
        }

        class PorkbunRequestDnsEditRecord
        {

            /// <summary>
            /// The answer content for the record. Please see the DNS management popup from the domain management console for proper formatting of each record type.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
            [JsonPropertyName("content")]
            public string Content { get; set; }

            /// <summary>
            /// The time to live in seconds for the record. The minimum and the default is 600 seconds.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("ttl")]
            public string Ttl { get; set; }


            /// <summary>
            /// The priority of the record for those that support it.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("prio")]
            public string Prio { get; set; }


            /// <summary>
            /// Any notes that you'd like to set for the record.
            /// </summary>
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("notes")]
            public string Notes { get; set; }
        }

        class PorkbunResponseRetireveRecords
        {
            public string Status { get; set; }

            public IList<Record> Records { get; set; }

            public class Record
            {
                public string Id { get; set; }
                public string Name { get; set; }
                public string Type { get; set; }
                public string Content { get; set; }
                public string Ttl { get; set; }
                public string Prio { get; set; }
                public string Notes { get; set; }
            }
        }
    }
}

/*

https://porkbun.com/api/json/v3/documentation
DNS Edit Record by Domain, Subdomain and Type
 
URI Endpoint: https://api.porkbun.com/api/json/v3/dns/editByNameType/DOMAIN/TYPE/[SUBDOMAIN]
URI Endpoint Example: https://api.porkbun.com/api/json/v3/dns/editByNameType/borseth.ink/A/www

JSON Command Example

{
	"secretapikey": "YOUR_SECRET_API_KEY",
	"apikey": "YOUR_API_KEY",
	"content": "1.1.1.2",
	"ttl": "600"
}

JSON Response Example

{
	"status": "SUCCESS"
}
 */