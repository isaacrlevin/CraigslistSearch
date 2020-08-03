using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CraigslistSearch.Shared.Models;
using CraigslistSearch.Function.Services;
namespace CraigslistSearch.Function
{
    public class Functions
    {
        private readonly SearchService _service;

        public Functions(SearchService service)
        {
            _service = service;
        }

        [FunctionName("GetResults")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var filter = JsonConvert.DeserializeObject<Filter>(requestBody);
            var SearchResults = await _service.GetItems(filter);
            return new OkObjectResult(SearchResults);
        }
    }
}
