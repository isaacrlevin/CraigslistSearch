using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraigslistSearch.Web.Models
{
    public class SearchResponse
    {
        public string Body { get; set; }
        public DateTime? TimeStampDate { get; set; }
        public string Title { get; set; }
        public float Price { get; set; }
        public string Location { get; set; }
        public string ExternalUrl { get; set; }
    }

    public class Filter
    {
        public int Age { get; set; }
        public decimal[] Amount { get; set; }
        public string Location { get; set; }
        public string Category { get; set; }
        public string SearchText { get; set; }
    }
}
