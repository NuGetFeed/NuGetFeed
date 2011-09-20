using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Norm;
using NuGetFeed.Infrastructure.ActionResults;
using NuGetFeed.Models;
using NuGetFeed.NuGetService;
using NuGetFeed.ViewModels;

namespace NuGetFeed.Controllers
{
    [Authorize]
    public class FeedController : Controller
    {
        private readonly IMongo _mongo;
        private readonly ListController _listController;

        public FeedController(IMongo mongo, ListController listController)
        {
            _mongo = mongo;
            _listController = listController;
        }

        public ActionResult Index()
        {
            var users = _mongo.GetCollection<User>();
            var feeds = _mongo.GetCollection<Feed>();
            var context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));

            var currentUser = users.AsQueryable().Single(u => u.Username == User.Identity.Name);
            var feed = feeds.AsQueryable().SingleOrDefault(f => f.User == currentUser.Id);

            var packages = new List<PublishedPackage>();
            foreach (var package in feed.Packages)
            {
                var result = context.Packages.Where(p => p.Id == package && p.IsLatestVersion).SingleOrDefault();
                if(result != null)
                {
                    packages.Add(result);
                }
            }
            
            var viewModel = new MyFeedViewModel
                                {
                                    FeedId = feed.Id.ToString(),
                                    Packages = packages.OrderBy(x => x.Title).ToList()
                                };

            return View(viewModel);
        }

        public ActionResult Rss(string id)
        {
            var feeds = _mongo.GetCollection<Feed>();
            var feed = feeds.AsQueryable().Single(f => f.Id == new ObjectId(id));

            var feedString = string.Join(",", feed.Packages);

            return _listController.Packages(feedString);
        }

        public ActionResult RemovePackage(string id)
        {
            var users = _mongo.GetCollection<User>();
            var feeds = _mongo.GetCollection<Feed>();

            var currentUser = users.AsQueryable().Single(u => u.Username == User.Identity.Name);
            var feed = feeds.AsQueryable().SingleOrDefault(f => f.User == currentUser.Id);
            if (feed != null)
            {
                feed.Packages.Remove(id.ToLowerInvariant());
                feeds.Save(feed);
            }

            return RedirectToAction("Index");
        }

        public ActionResult AddToMyFeed(string id)
        {
            var users = _mongo.GetCollection<User>();
            var feeds = _mongo.GetCollection<Feed>();

            var currentUser = users.AsQueryable().Single(u => u.Username == User.Identity.Name);

            var feed = feeds.AsQueryable().SingleOrDefault(f => f.User == currentUser.Id);
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

            feeds.Save(feed);
            
            return RedirectToAction("Index");
        }
    }
}
