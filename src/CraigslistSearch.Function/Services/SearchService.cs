using CraigslistSearch.Shared.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml;

namespace CraigslistSearch.Function.Services
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
        private readonly HttpClient Http;

        public SearchService(HttpClient _http)
        {
            Http = _http;
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
        public async Task<List<SearchResponse>> GetItems(Filter filter)
        {
            var list = System.Text.Json.JsonSerializer.Deserialize<List<Location>>(await Http.GetStringAsync("https://craigslistsearch.blob.core.windows.net/json/locations.json"));

            List<SearchResponse> clData = new List<SearchResponse>();
            DateTime now = DateTime.Now;
            ConcurrentDictionary<string, SearchResponse> unique = new ConcurrentDictionary<string, SearchResponse>();

            if (filter.Location.Count > 0 && !filter.Location.Contains("all"))
            {
                list = list.Where(a => filter.Location.Contains(a.City)).ToList();
            }



            //await GetResults(unique, filter, list).AsyncParallelForEach(async entry =>
            //{
            //    clData.AddRange(entry);
            //}, 1000, TaskScheduler.FromCurrentSynchronizationContext()
            //);

            clData = GetResults(unique, filter, list);


            double span = DateTime.Now.Subtract(now).TotalSeconds;
            DateTime date = DateTime.Now.AddDays(Convert.ToInt32(filter.Age) * -1);

            return clData.Where(a => a.TimeStampDate >= date).OrderBy(a => a.Location).ThenByDescending(a => a.TimeStampDate).ToList();
        }

        //private async IAsyncEnumerable<List<SearchResponse>> GetResults(ConcurrentDictionary<string, SearchResponse> unique, Filter userVM, List<Location> list)
        private List<SearchResponse> GetResults(ConcurrentDictionary<string, SearchResponse> unique, Filter userVM, List<Location> list)
        {
            HttpClient client = new HttpClient();
            List<SearchResponse> clData = new List<SearchResponse>();
            Parallel.ForEach(list, str =>
            {
                Console.WriteLine($"Searching : {str.City}");
                string url;
               
                try
                {
                    if (str != null)
                    {
                        url = str.Url.Replace(" ", string.Empty) + "search/" + (userVM.Category ?? "sss") + "/?query=" + userVM.SearchText + "&format=rss";


                        var rss =  client.GetStreamAsync(url).Result;

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
            });
            return clData.Where(a => a != null).ToList();
        }
    }
}
