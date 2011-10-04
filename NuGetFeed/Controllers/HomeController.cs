using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NuGetFeed.Infrastructure.PackageSources;
using NuGetFeed.Infrastructure.Repositories;
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

        public ActionResult MostPopular()
        {
            var totalFeeds = _feedRepository.GetNumberOfFeeds();
            var popular = _feedRepository.MostPopular().ToList();

            var listOfPackages = new List<MostPopularViewModel>();
            foreach (var package in popular)
            {
                listOfPackages.Add(new MostPopularViewModel()
                                       {
                                           Package = _nuGetOrgFeed.GetLatestVersion(package.Id),
                                           IncludedInFeeds = (decimal)package.value / totalFeeds * 100,
                                       });
            }

            return View(listOfPackages);
        }
    }
}
