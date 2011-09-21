using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NuGetFeed.NuGetService;
using NuGetFeed.ViewModels;

namespace NuGetFeed.Controllers
{
    public class PackagesController : Controller
    {
        public ActionResult Index(string query)
        {
            var model = new PackageSearchViewModel();

            if (!string.IsNullOrWhiteSpace(query))
            {
                model.Query = query;

                var context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));
                var packages = from p in context.Packages
                               where (p.Id.Contains(query)
                                     || p.Description.Contains(query)
                                     || p.Tags.Contains(query)
                                     || p.Title.Contains(query))
                                     && p.IsLatestVersion
                               orderby p.DownloadCount descending
                               select p;

                model.Packages = packages.ToList();
            }

            return View(model);
        }

    }
}
