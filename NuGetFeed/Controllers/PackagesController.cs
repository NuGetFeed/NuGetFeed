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
        public ActionResult Index(string query, int? page)
        {
            var model = new PackageSearchViewModel();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var startFrom = page.HasValue && page.Value > 0 ? (page.Value - 1)*10 : 0;

                var context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));
                var packages = context.Packages.Where(p => (p.Id.Contains(query)
                                                            || p.Description.Contains(query)
                                                            || p.Tags.Contains(query)
                                                            || p.Title.Contains(query))
                                                           && p.IsLatestVersion);

                var packageCount = packages.Count();
                model.Query = query;
                model.CurrentPage = page.HasValue && page.Value > 0 ? page.Value : 1;
                model.TotalPages = (packageCount / 10) + (packageCount % 10 > 0 ? 1 : 0);
                model.Packages = packages.OrderByDescending(x => x.DownloadCount).Skip(startFrom).Take(10).ToList();
            }

            return View(model);
        }

    }
}
