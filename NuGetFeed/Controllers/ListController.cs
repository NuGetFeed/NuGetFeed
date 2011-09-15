using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;
using NuGetFeed.NuGetService;

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
            
            var feed = CreateFeed("Recent NuGet package releases of " + id, id);
            feed.Items = allItems;

            return new RssActionResult { Feed = feed };
        }

        private IEnumerable<SyndicationItem> CreateListOfItems(string packageId)
        {
            var context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));
            var packages = (from p in context.Packages
                           where p.Id == packageId
                           orderby p.LastUpdated descending
                           select p).Take(5);

            foreach (var p in packages)
            {
                var item = new SyndicationItem(p.Title + " " + p.Version, string.Empty, new Uri(p.GalleryDetailsUrl), p.Id + p.Version, p.LastUpdated)
                               {
                                   Content = new TextSyndicationContent(p.Title + " version " + p.Version + " released."),
                                   PublishDate = p.LastUpdated
                               };

                yield return item;
            }
        }

        private static SyndicationFeed CreateFeed(string description, string id)
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
