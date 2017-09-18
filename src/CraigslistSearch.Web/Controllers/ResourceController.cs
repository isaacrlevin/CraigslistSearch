using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using CraigslistSearch.Web.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    public class ResourceController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public ResourceController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }


        [HttpGet("[action]")]
        public IEnumerable<Category> GetCategories()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string json = System.IO.File.ReadAllText($"{webRootPath}/categories.json");
            return JsonConvert.DeserializeObject<List<Category>>(json);
        }

        [HttpGet("[action]")]
        public IEnumerable<Location> GetLocations()
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string json = System.IO.File.ReadAllText($"{webRootPath}/locations.json");
            return JsonConvert.DeserializeObject<List<Location>>(json);
        }


    }
}
