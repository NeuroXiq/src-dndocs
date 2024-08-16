using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.SitemapXml
{
    public class SitemapIndexGenerator
    {
        const int MaxUrlsCount = 50000;
        const int MaxSitemapIndexSize = 10 * 1000 * 1000;
        const string SitemapFormat = "<sitemap> <loc>{0}</loc> <lastmod>{1}</lastmod> </sitemap>";

        public int UrlsCount => urlsCount;

        private StringBuilder sb;
        int urlsCount;

        public SitemapIndexGenerator()
        {
            sb = new StringBuilder();
            Clear();
        }

        public void Append(string url, DateTime lastMod)
        {
            sb.AppendFormat(SitemapFormat, url, SitemapGenerator.FormatLastMod(lastMod));

            urlsCount++;

            Validate();
        }

        public void Clear()
        {
            sb.Clear();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            // vs https://www.sitemaps.org/schemas/sitemap/0.9 (https not working with google search console??, not sure why)
            sb.AppendLine("<sitemapindex xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            urlsCount = 0;
        }

        public string ToStringXmlAndClear()
        {
            sb.AppendLine("</sitemapindex>");

            var result = sb.ToString();
            
            return result;
        }

        private void Validate()
        {
            if (sb.Length > MaxSitemapIndexSize) throw new InvalidOperationException("sitemapindex exceed max size in bytes");
            if (urlsCount > MaxUrlsCount) throw new InvalidOperationException("sitemapindex too many urls");
        }
    }
}
