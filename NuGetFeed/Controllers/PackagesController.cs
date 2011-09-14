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
        public ActionResult Index(string searchTerm)
        {
            var model = new PackageSearchViewModel();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                model.SearchTerm = searchTerm;

                var context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));
                var packages = from p in context.Packages
                               where (p.Id.Contains(searchTerm)
                                     || p.Description.Contains(searchTerm)
                                     || p.Tags.Contains(searchTerm)
                                     || p.Title.Contains(searchTerm))
                                     && p.IsLatestVersion
                               orderby p.DownloadCount descending
                               select p;

                model.Packages = packages.ToList();
            }

            return View(model);
        }

    }
}
