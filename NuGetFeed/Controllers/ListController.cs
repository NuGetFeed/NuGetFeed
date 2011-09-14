using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml;

namespace NuGetFeed.Controllers
{
    public class ListController : Controller
    {
        public ActionResult Packages(string id)
        {
            var allItems = new List<SyndicationItem>();
            var packages = id.Split(',');
            foreach (string package in packages)
            {
                allItems.AddRange(CreateListOfItems(package.Trim()));
            }

            allItems = allItems.OrderByDescending(x => x.LastUpdatedTime).ToList();
            
            var feed = CreateFeed("Recent releases of " + id, "Recent NuGet package releases of " + id, id);
            feed.Items = allItems;

            return new RssActionResult { Feed = feed };
        }

        private IList<SyndicationItem> CreateListOfItems(string packageId)
        {
            var html = new WebClient();
            string downloadString = html.DownloadString("http://nuget.org/List/Packages/" + packageId);

            var list = new List<SyndicationItem>();

            int begin = 0;

            while (downloadString.IndexOf("<td class=\"version\">", begin) != -1)
            {
                // Version
                int start = downloadString.IndexOf("<td class=\"version\">", begin) + 20;
                int end = downloadString.IndexOf("</td>", start);
                var version = downloadString.Substring(start, end - start);
                version = Regex.Replace(version, @"<[^>]*>", string.Empty).Trim();

                // Date
                int dateStart = downloadString.IndexOf("<td class=\"lastUpdated\">", end) + 24;
                int dateEnd = downloadString.IndexOf("</td>", dateStart);
                string updatedDate = downloadString.Substring(dateStart, dateEnd - dateStart).Trim();

                DateTime date;
                if (updatedDate.Length == 11)
                {
                    date = DateTime.ParseExact(updatedDate, "dd MMM yyyy", null);
                }
                else
                {
                    date = DateTime.ParseExact(updatedDate, "d MMM yyyy", null);
                }

                list.Add(CreateFeedItem(version, version, "http://nuget.org/List/Packages/" + packageId + "/" + version.Substring(version.LastIndexOf(' ') + 1), packageId + version, date));

                begin = end;
            }

            return list;
        }

        private static SyndicationItem CreateFeedItem(string title, string text, string url, string id, DateTime created)
        {
            var item = new SyndicationItem(title, text, new Uri(url), id, created);
            return item;
        }

        private static SyndicationFeed CreateFeed(string title, string description, string id)
        {
            var feed = new SyndicationFeed(string.Format("nugetfeed.org - {0}", id), description, new Uri("http://nugetfeed.org"), id, DateTime.Now)
            {
                Language = "EN",
                Copyright = new TextSyndicationContent("Copyright " + DateTime.Today.Year + ", nugetfeed.org")
            };
            return feed;
        }
    }

    public class RssActionResult : ActionResult
    {
        public SyndicationFeed Feed { get; set; }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/rss+xml";

            var formatter = new Rss20FeedFormatter(Feed);
            using (var writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                formatter.WriteTo(writer);
            }
        }
    }
}
