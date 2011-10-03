using System.Linq;
using System.Web.Mvc;
using NuGetFeed.Infrastructure.Repositories;

namespace NuGetFeed.Controllers
{
    public class HomeController : Controller
    {
        private readonly FeedRepository _feedRepository;

        public HomeController(FeedRepository feedRepository)
        {
            _feedRepository = feedRepository;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MostPopular()
        {
            var popular = _feedRepository.MostPopular().ToList();
            return View(popular);
        }
    }
}
