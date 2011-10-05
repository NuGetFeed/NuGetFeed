using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NuGetFeed.Infrastructure.PackageSources;
using NuGetFeed.Infrastructure.Repositories;
using NuGetFeed.NuGetService;
using NuGetFeed.ViewModels;

namespace NuGetFeed.Controllers
{
    public class HomeController : Controller
    {
        private readonly FeedRepository _feedRepository;
        private readonly NuGetOrgFeed _nuGetOrgFeed;

        public HomeController(FeedRepository feedRepository, NuGetOrgFeed nuGetOrgFeed)
        {
            _feedRepository = feedRepository;
            _nuGetOrgFeed = nuGetOrgFeed;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MostFollowed()
        {
            var totalFeeds = _feedRepository.GetNumberOfFeeds();
            var popular = _feedRepository.MostFollowed(25).ToList();

            var listOfPackages = new List<MostFollowedViewModel>();
            foreach (var package in popular)
            {
                listOfPackages.Add(new MostFollowedViewModel()
                                       {
                                           Package = _nuGetOrgFeed.GetLatestVersion(package.Id),
                                           IncludedInFeeds = (decimal)package.value / totalFeeds * 100,
                                       });
            }

            return View(listOfPackages);
        }

        [OutputCache(Duration = 43200)]
        public PartialViewResult FrontpageMostFollowed()
        {
            var popular = _feedRepository.MostFollowed(10).ToList();
            var list = popular.Select(p => _nuGetOrgFeed.GetLatestVersion(p.Id)).ToList();

            return PartialView(list);
        }

        [OutputCache(Duration = 3600)]
        public PartialViewResult FrontpageNewReleases()
        {
            var releases = _nuGetOrgFeed.GetAllByDescendingPublishDate().Take(10).ToList();

            return PartialView(releases);
        }
    }
}
