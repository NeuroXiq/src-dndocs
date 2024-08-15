using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Vinca.SitemapXml
{
    public class SitemapGenerator
    {
        const int MaxUrlsCount = 50000;
        const int MaxSitemapFileSizeBytes = 50 * 1000 * 1000;
        
        public int UrlsCount => urlsCount;

        /// <summary>
        /// this length does not includue few bytes (closing sitemap xml tags)
        /// </summary>
        public int CurrentLength => sb.Length;

        StringBuilder sb;
        int urlsCount;

        // todo, maybe change values to nullable to not force e.g. changefreq
        const string UrlEntryFormat = "<url>" +
                "<loc>{0}</loc>" +
                "<lastmod>{1}</lastmod>" +
                "<changefreq>{2}</changefreq>" +
                // "<priority>{3}</priority>" +
                "</url>";

        public SitemapGenerator()
        {
            this.sb = new StringBuilder();
            Clear();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        }


        public bool CanAppend(IList<string> urls, DateTime lastMod, ChangeFreq changeFreq)
        {
            var urlsLength = urls.Sum(t => t.Length);
            var entryWithoutUrl = string.Format(UrlEntryFormat, "", lastMod, changeFreq);

            bool sizeNotExceed = (sb.Length  + urlsLength + (urls.Count * entryWithoutUrl.Length)) <= MaxSitemapFileSizeBytes;
            bool urlsCountNotExceed = UrlsCount + urls.Count < MaxUrlsCount;

            return sizeNotExceed && urlsCountNotExceed;

        }

        public void Append(IList<string> urls, DateTime lastMod, ChangeFreq changeFreq)
        {
            var changeFreqString = FormatChangeFreq(changeFreq);
            var lastModString = FormatLastMod(lastMod);
            
            foreach (var url in urls) sb.AppendFormat(UrlEntryFormat, url, lastModString, changeFreqString);

            urlsCount += urls.Count;

            ThrowIfInvalid();
        }

        static string FormatChangeFreq(ChangeFreq changeFreq)
        {
            var cfs = "";
            switch (changeFreq)
            {
                case ChangeFreq.Always: cfs = "always"; break;
                case ChangeFreq.Hourly: cfs = "hourly"; break;
                case ChangeFreq.Daily: cfs = "daily"; break;
                case ChangeFreq.Weekly: cfs = "Weekly"; break;
                case ChangeFreq.Monthly: cfs = "monthly"; break;
                case ChangeFreq.Yearly: cfs = "yearly"; break;
                case ChangeFreq.Never: cfs = "never"; break;
                default: throw new ArgumentException("changefreq");
            }


            return cfs;
        }

        /// <summary>
        /// </summary>
        /// <param name="loc">URL must be valid url escaped </param>
        /// <param name="lastMod">last modified (optional)</param>
        /// <param name="changeFreq">change frequency (optional)</param>
        /// <param name="priority">priority (optional)</param>
        /// <returns></returns>
        //public void Append(string loc, DateTime? lastMod, ChangeFreq? changeFreq, double? priority)
        //{
        //    if (priority.HasValue && priority < 0 || priority > 1)
        //        throw new ArgumentException("priority not in range 0.0 - 1.0)");
        //    if (string.IsNullOrWhiteSpace(loc)) throw new ArgumentException(nameof(loc));

        //    string cfs = null;

        //    if (changeFreq.HasValue)
        //    {
                
        //    }

        //    sb.AppendLine("<url>");
        //    sb.AppendLine($"\\t<loc>{loc}</loc>");
        //    if (lastMod.HasValue) sb.AppendLine($"\t\t<lastmod>{LastMod(lastMod.Value)}</lastmod>");
        //    if (cfs != null) sb.AppendLine($"\t\t<changefreq>{cfs}</changefreq>");
        //    if (priority.HasValue) sb.AppendLine($"\t\t<priority>{priority.Value}</priority>");
        //    sb.AppendLine("\t</url>");

        //    urlsCount++;
        //}

        public void ThrowIfInvalid()
        {
            // TODO: this need to include last append: '</urlset>' fromn toxmlstringandclera

            // just estimated for now, this is not exact to fit 10MB afte append
            if (sb.Length > MaxSitemapFileSizeBytes)
                throw new InvalidDataException($"not valid length of sitemap.xml after generation. current {sb.Length} bytes, max sitemap length: {MaxSitemapFileSizeBytes} bytes");
            if (urlsCount > MaxUrlsCount)
                throw new InvalidDataException($"max urls count exceed limit. count: {urlsCount}, limit for sitemap file: {MaxUrlsCount}");
        }

        public bool IsValid()
        {
            try
            {
                ThrowIfInvalid();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Clear()
        {
            sb.Clear();
            urlsCount = 0;
        }

        public string ToXmlStringAndClear()
        {
            sb.AppendLine("</urlset>");
            var result = sb.ToString();

            Clear();

            return result;
        }

        internal static string FormatLastMod(DateTime datetime)
        {
            return datetime.ToString("yyyy'-'MM'-'dd");
        }
    }
}
