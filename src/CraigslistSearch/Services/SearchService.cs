using CraigslistSearch.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.ServiceModel.Syndication;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;

namespace CraigslistSearch.Services
{
    public static class Extenstions
    {
        public static async Task AsyncParallelForEach<T>(this IAsyncEnumerable<T> source, Func<T, Task> body, int maxDegreeOfParallelism = DataflowBlockOptions.Unbounded, TaskScheduler scheduler = null)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            };
            if (scheduler != null)
                options.TaskScheduler = scheduler;

            var block = new ActionBlock<T>(body, options);

            await foreach (var item in source)
                block.Post(item);

            block.Complete();
            await block.Completion;
        }
    }

    public class SearchService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SearchService(IWebHostEnvironment hostingEnvironment)
        {

            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<List<SearchResponse>> GetItems(Filter filter)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string json = System.IO.File.ReadAllText($"{webRootPath}/locations.json");
            var list = JsonSerializer.Deserialize<List<Location>>(json);

            List<SearchResponse> clData = new List<SearchResponse>();
            DateTime now = DateTime.Now;
            ConcurrentDictionary<string, SearchResponse> unique = new ConcurrentDictionary<string, SearchResponse>();

            if (filter.Location.Count > 0 && !filter.Location.Contains("all"))
            {
                list = list.Where(a => filter.Location.Contains(a.City)).ToList();
            }

            await GetResults(unique, filter, list).AsyncParallelForEach(async entry =>
            {                
                clData.AddRange(entry);
            }, 1000, TaskScheduler.FromCurrentSynchronizationContext()
            );

            double span = DateTime.Now.Subtract(now).TotalSeconds;
            DateTime date = DateTime.Now.AddDays(Convert.ToInt32(filter.Age) * -1);

            return clData.Where(a => a.TimeStampDate >= date).OrderBy(a => a.Location).ThenByDescending(a => a.TimeStampDate).ToList();
        }

        private async IAsyncEnumerable<List<SearchResponse>> GetResults(ConcurrentDictionary<string, SearchResponse> unique, Filter userVM, List<Location> list)
        {
            HttpClient client = new HttpClient();
            foreach (var str in list)
            {
                Console.WriteLine($"Searching : {str.City}");
                string url;
                List<SearchResponse> clData = new List<SearchResponse>();
                try
                {
                    if (str != null)
                    {
                        url = str.Url.Replace(" ", string.Empty) + "search/" + (userVM.Category ?? "sss") + "/?query=" + userVM.SearchText + "&format=rss";


                        var rss = await client.GetStreamAsync(url);

                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(rss);
                        XmlNodeList nodeList = xmlDoc.GetElementsByTagName("item");
                        Regex pattern = new Regex("&(.*?);\\d*");

                        Parallel.ForEach(nodeList.Cast<XmlNode>().ToList(), xml =>
                        {
                            var childNodes = xml.ChildNodes.Cast<XmlNode>().ToList();

                            SearchResponse data = new SearchResponse
                            {
                                Location = str.City,
                            };

                            Parallel.ForEach(childNodes, child =>
                            {
                                switch (child.Name)
                                {
                                    case "title":
                                        string newString = new string(child.InnerText.Where(c => c <= sbyte.MaxValue).ToArray());
                                        data.Title = pattern.Replace(newString, "");
                                        break;
                                    case "link":
                                        data.ExternalUrl = child.InnerText;
                                        break;
                                    case "description":
                                        data.Body = child.InnerText;
                                        break;
                                    case "dc:date":
                                        data.TimeStampDate = DateTime.Parse(child.InnerText).ToUniversalTime();
                                        break;
                                    default:
                                        break;
                                }
                            });

                            if (!unique.ContainsKey(data.Location + data.Title))
                            {
                                clData.Add(data);
                                unique.AddOrUpdate(data.Location + data.Title, data, (key, oldValue) => data);
                            }
                        });
                    }
                }
                catch (Exception e)
                {
                    var foo = e;
                }
                yield return clData.Where(a => a != null).ToList() ;
            }
        }
    }
}
