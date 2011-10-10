using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Web.Routing;
using NuGetFeed.NuGetService;

namespace NuGetFeed.Infrastructure.Rss
{
    public class SyndicationHelper
    {
        public SyndicationItem CreateNuGetPackageSyndicationItem(PublishedPackage package)
        {
            var title = package.Title + " " + package.Version;
            var link = "http://nugetfeed.org/list/packages/" + package.Id.ToLower() + "/details?utm_source=ngf&utm_medium=rss&utm_campaign=rss_feeds";
            var id = package.Id + package.Version;
            var updated = package.LastUpdated;

            var content = "<p><strong>Description</strong></p><p>" + package.Description + "</p>";

            if (!string.IsNullOrWhiteSpace(package.ReleaseNotes))
            {
                content += "<p><strong>Release notes</strong></p><p>" + package.ReleaseNotes + "</p>";
            }

            var htmlContent = SyndicationContent.CreateHtmlContent(content);

            var item = new SyndicationItem(title, htmlContent, new Uri(link), id, updated)
                           {
                               PublishDate = package.LastUpdated,
                           };

            return item;
        }

        public SyndicationFeed CreateFeed(string title, string description, string uniqueId, IEnumerable<SyndicationItem> items)
        {
            var feed = new SyndicationFeed(title, description, new Uri("http://nugetfeed.org"), uniqueId, DateTimeOffset.Now, items)
                           {
                               Language = "EN",
                               Copyright = new TextSyndicationContent("Copyright " + DateTime.Today.Year + ", nugetfeed.org")
                           };

            return feed;
        }
    }
}