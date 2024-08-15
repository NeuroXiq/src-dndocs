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

        const string UrlSetClosingTag = "</urlset>";
        // todo, maybe change values to nullable to not force e.g. changefreq
        const string UrlEntryFormat = "<url>" +
                "<loc>{0}</loc>" +
                "<lastmod>{1}</lastmod>" +
                "<changefreq>{2}</changefreq>" +
                // "<priority>{3}</priority>" +
                "</url>";

        public SitemapGenerator()
        {
            sb = new StringBuilder();
            Clear();
        }


        // public bool CanAppend(IList<string> urls, DateTime lastMod, ChangeFreq changeFreq)
        // {
        // 
        //     bool urlsCountNotExceed = UrlsCount + urls.Count < MaxUrlsCount;
        // 
        //     // return sizeNotExceed && urlsCountNotExceed;
        //     throw new Exception();
        // 
        // }

        public bool TryAppend(IList<string> urls, DateTime lastMod, ChangeFreq changeFreq)
        {
            var entryWithoutUrl = string.Format(UrlEntryFormat, "", lastMod, changeFreq);
            var urlsLength = urls.Sum(t => t.Length);

            bool urlsWillExceed = urls.Count + urlsCount > MaxUrlsCount;
            bool sizeWillExceed = (sb.Length + urlsLength + (urls.Count * entryWithoutUrl.Length)) > MaxSitemapFileSizeBytes;

            // final sitemap will be larger than this so if right now is bigger does not make sens to continue
            // (urls will be encoded for xml, so can be little larger later)
            // do this preemptively because dont want to XmlEscape large amount of strings
            if (urlsWillExceed || sizeWillExceed) return false;
            
            var changeFreqString = FormatChangeFreq(changeFreq);
            var lastModString = FormatLastMod(lastMod);
            int oldLength = sb.Length;

            foreach (var url in urls)
            {
                var xmlEscapedUrl = XmlEscapeUrl(url);
                sb.AppendFormat(UrlEntryFormat, xmlEscapedUrl, lastModString, changeFreqString);
            }

            urlsCount += urls.Count;

            // as last step will append closing tag, so length need to be included here
            if (sb.Length + UrlSetClosingTag.Length > MaxSitemapFileSizeBytes || urlsCount > MaxUrlsCount)
            {
                // revert changes because exceed
                sb.Length = oldLength;
                urlsCount -= urls.Count;
                return false;
            }

            return true;
        }

        private string XmlEscapeUrl(string url)
        {
            string result = url;
            result = result.Replace("&", "&amp;");
            result = result.Replace("<", "&lt;");
            result = result.Replace(">", "&gt;");
            result = result.Replace("\"", "&quot;");
            result = result.Replace("\'", "&apos;");

            return result;
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

        public void Clear()
        {
            sb.Clear();
            urlsCount = 0;
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            sb.AppendLine("<urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
        }

        public string ToXmlStringAndClear()
        {
            sb.AppendLine(UrlSetClosingTag);
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
