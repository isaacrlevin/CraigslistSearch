using CraigslistSearch.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace CraigslistSearch.Web
{
    public interface ISearchService
    {
        List<SearchResponse> GetItems(Filter filter);
    }


    public class SearchService : ISearchService
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public SearchService(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public List<SearchResponse> GetItems(Filter filter)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string json = System.IO.File.ReadAllText($"{webRootPath}/locations.json");
            var list = JsonConvert.DeserializeObject<List<Location>>(json);

            List<SearchResponse> clData = new List<SearchResponse>();
            DateTime now = DateTime.Now;
            ConcurrentDictionary<string, SearchResponse> unique = new ConcurrentDictionary<string, SearchResponse>();
            if (!string.IsNullOrEmpty(filter.Location) && filter.Location.ToLower() != "all")
            {
                clData.AddRange(GetResults(unique, filter, list.Where(a => a.City == filter.Location).FirstOrDefault()));
            }
            else
            {
                Parallel.ForEach(list, str =>
                {
                    clData.AddRange(GetResults(unique, filter, str));
                });
            }
            double span = DateTime.Now.Subtract(now).TotalSeconds;
            DateTime date = DateTime.Now.AddDays(Convert.ToInt32(filter.Age) * -1);

            return clData.Where(a => a.TimeStampDate >= date).ToList();
        }

        private List<SearchResponse> GetResults(ConcurrentDictionary<string, SearchResponse> unique, Filter userVM, Location str)
        {
            string url;
            List<SearchResponse> clData = new List<SearchResponse>();
            try
            {
                if (str != null)
                {

                    url = str.Url.Replace(" ", string.Empty) + "search/" + (userVM.Category == null ? "sss" : userVM.Category) + "/?query=" + userVM.SearchText + "&format=rss";
                    XmlReader reader = XmlReader.Create(url);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(reader);
                    XmlNodeList nodeList = xmlDoc.GetElementsByTagName("item");
                    Regex pattern = new Regex("&(.*?);\\d*");
                    Parallel.ForEach(nodeList.Cast<XmlNode>().ToList(), xml =>
                    {
                        int itemCt = 1;
                        SearchResponse data = new SearchResponse();
                        data.Location = str.City;
                        if (itemCt <= 5)
                        {
                            Parallel.ForEach(xml.ChildNodes.Cast<XmlNode>().ToList(), child =>
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
                                itemCt++;
                                unique.AddOrUpdate(data.Location + data.Title, data, (key, oldValue) => data);
                            }
                        }
                    });
                }
            }
            catch
            { }
            return clData;
        }
    }
}
