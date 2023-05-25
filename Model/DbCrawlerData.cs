using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler.Model
{
    public class DbCrawlerData
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int NumPages { get; set; }
        public int NumLines { get; set; }
        public string? Json { get; set; }
    }
}
