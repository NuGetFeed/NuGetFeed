using System.Linq;
using System.Web.Mvc;

using NuGetFeed.Infrastructure.AutoMapper;
using NuGetFeed.Infrastructure.PackageSources;
using NuGetFeed.ViewModels;

namespace NuGetFeed.Controllers
{
    public class PackagesController : Controller
    {
        private readonly NuGetOrgFeed _nuGetOrgFeed;

        public PackagesController(NuGetOrgFeed nuGetOrgFeed)
        {
            _nuGetOrgFeed = nuGetOrgFeed;
        }

        public ActionResult Index(string query, int? page)
        {
            var model = new PackageSearchViewModel();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var startFrom = page.HasValue && page.Value > 0 ? (page.Value - 1) * 10 : 0;
                int packageCount;
                var packages = _nuGetOrgFeed.Search(query, startFrom, 10, out packageCount);

                model.Query = query;
                model.CurrentPage = page.HasValue && page.Value > 0 ? page.Value : 1;
                model.TotalPages = (packageCount / 10) + (packageCount % 10 > 0 ? 1 : 0);
                model.Packages = packages.ToList();
            }

            return View(model);
        }

        public ActionResult Details(string id)
        {
            var package = this._nuGetOrgFeed.GetLatestVersion(id);
            if (package == null)
            {
                return HttpNotFound();
            }

            var model = package.MapToDynamic<PackageViewModel>();
            return View(model);
        }
    }
}
