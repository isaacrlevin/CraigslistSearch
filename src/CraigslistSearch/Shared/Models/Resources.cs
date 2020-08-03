using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CraigslistSearch.Shared.Models
{
    public class Category
    {
        public string Description { get; set; }
        public string Abbreviation { get; set; }

    }

    public class Location
    {
        public string City { get; set; }
        public string Url { get; set; }
    }
}
