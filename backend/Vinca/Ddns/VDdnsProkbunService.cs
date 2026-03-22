using Microsoft.Extensions.Options;

namespace Vinca.Ddns
{
    public class VDdnsPorkbunService : IVDdnsService
    {
        private VDdnsProkbunServiceOptions options;

        public VDdnsPorkbunService(IOptions<VDdnsProkbunServiceOptions> options)
        {
            this.options = options.Value;
        }

        public Task UpdateDdns(CancellationToken token)
        {
            return Task.CompletedTask;
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