using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRssReader
{
    public class Feed
    {
        public string id { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string view { get; set; }
        public string policy { get; set; }
    }
}
