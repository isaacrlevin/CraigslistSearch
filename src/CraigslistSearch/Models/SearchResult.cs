using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CraigslistSearch.Models
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
        [Required, MinLength(1, ErrorMessage = "At least one location required")]
        public List<string> Location { get; set; }
        public string Category { get; set; }
        [Required]
        public string SearchText { get; set; }
    }
}
