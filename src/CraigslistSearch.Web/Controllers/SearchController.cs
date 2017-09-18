using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using CraigslistSearch.Web;
using CraigslistSearch.Web.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        private readonly ISearchService _searchService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public SearchController(IHostingEnvironment hostingEnvironment, ISearchService searchService)
        {
            _hostingEnvironment = hostingEnvironment;
            _searchService = searchService;
        }

        [HttpPost]
        public IEnumerable<SearchResponse> Index([FromBody] Filter filter)
        {
            return _searchService.GetItems(filter);
        }
    }
}
