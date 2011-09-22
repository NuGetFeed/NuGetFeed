using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Norm;
using NuGetFeed.Infrastructure.Repositories;
using NuGetFeed.Models;
using NuGetFeed.NuGetService;
using NuGetFeed.ViewModels;

namespace NuGetFeed.Controllers
{
    public class FeedController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly FeedRepository _feedRepository;
        private readonly ListController _listController;

        public FeedController(UserRepository userRepository, FeedRepository feedRepository, ListController listController)
        {
            _userRepository = userRepository;
            _feedRepository = feedRepository;
            _listController = listController;
        }

        [Authorize]
        public ActionResult Index()
        {
            var context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));

            var currentUser = _userRepository.GetByUsername(User.Identity.Name);
            var feed = _feedRepository.GetByUser(currentUser);

            var packages = new List<PublishedPackage>();
            if (feed != null)
            {
                foreach (var package in feed.Packages)
                {
                    var result = context.Packages.Where(p => p.Id == package && p.IsLatestVersion).SingleOrDefault();
                    if (result != null)
                    {
                        packages.Add(result);
                    }
                }
            }

            var viewModel = new MyFeedViewModel
                                {
                                    FeedId = (feed != null ? feed.Id.ToString() : ""),
                                    Packages = packages.OrderBy(x => x.Title).ToList()
                                };

            return View(viewModel);
        }

        public ActionResult Rss(string id)
        {
            var feed = _feedRepository.GetById(new ObjectId(id));

            var feedString = string.Join(",", feed.Packages);

            return _listController.Packages(feedString);
        }

        [Authorize]
        public void RemovePackage(string id)
        {
            var currentUser = _userRepository.GetByUsername(User.Identity.Name);
            var feed = _feedRepository.GetByUser(currentUser);
            if (feed != null)
            {
                feed.Packages.Remove(id.ToLowerInvariant());
                _feedRepository.Save(feed);
            }
        }

        [Authorize]
        public string AddToMyFeed(string id)
        {
            var currentUser = _userRepository.GetByUsername(User.Identity.Name);

            var feed = _feedRepository.GetByUser(currentUser);
            if (feed == null)
            {
                feed = new Feed
                           {
                               User = currentUser.Id
                           };
            }

            if(!feed.Packages.Contains(id.ToLowerInvariant()))
            {
                feed.Packages.Add(id.ToLowerInvariant());
            }
            
            _feedRepository.Save(feed);

            return "<span class=\"label notice\">Added</span>";
        }
    }
}
