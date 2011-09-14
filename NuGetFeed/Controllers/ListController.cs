using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
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
            
            var feed = CreateFeed("Recent releases of " + id, "Recent NuGet package releases of " + id, id);
            feed.Items = allItems;

            return new RssActionResult { Feed = feed };
        }

        private IList<SyndicationItem> CreateListOfItems(string packageId)
        {
            //if (HttpContext.Cache[packageId.ToLower()] != null)
            //{
            //    return HttpContext.Cache[packageId.ToLower()] as List<SyndicationItem>;
            //}

            var context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));
            var packages = from p in context.Packages
                           where p.Id == packageId
                           select new SyndicationItem(p.Title + " " + p.Version, p.Title + " " + p.Version, new Uri(p.GalleryDetailsUrl), p.Id + p.Version, p.LastUpdated);

            //HttpContext.Cache.Add(packageId.ToLower(), packages.ToList(), null, DateTime.Now.AddMinutes(5), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
            return packages.ToList();
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
