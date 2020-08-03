using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CraigslistSearch.Shared.Models
{
    public class SearchResponse
    {
        [JsonPropertyName("body")]
        public string Body { get; set; }
        [JsonPropertyName("timeStampDate")]
        public DateTime? TimeStampDate { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        public float Price { get; set; }
        [JsonPropertyName("location")]
        public string Location { get; set; }
        [JsonPropertyName("externalUrl")]
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
