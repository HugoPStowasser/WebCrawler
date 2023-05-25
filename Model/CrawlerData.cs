using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Model
{
    public class CrawlerData
    {
        public string? IPAddress { get; set; }
        public string? Port { get; set; }
        public string? Country { get; set; }
        public string? Protocol { get; set; }
    }
}
