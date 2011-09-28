using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Norm;
using NuGetFeed.Infrastructure.ModelBinders;
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
        private readonly GalleryFeedContext _feed;
        private readonly UploadPackagesRequestRepository _uploadPackagesRequestRepository;

        public FeedController(UserRepository userRepository, FeedRepository feedRepository, ListController listController, UploadPackagesRequestRepository uploadPackagesRequestRepository, GalleryFeedContext feed)
        {
            _userRepository = userRepository;
            _feedRepository = feedRepository;
            _listController = listController;
            _uploadPackagesRequestRepository = uploadPackagesRequestRepository;
            _feed = feed;
        }

        [Authorize]
        public ActionResult Index()
        {
            if (TempData.ContainsKey("Message"))
            {
                ViewBag.MessageType = TempData.ContainsKey("MessageType") ? TempData["MessageType"] : "success";
                ViewBag.Message = TempData["Message"];
            }

            var currentUser = _userRepository.GetByUsername(User.Identity.Name);
            var feed = _feedRepository.GetByUser(currentUser);

            var packages = new List<PublishedPackage>();
            if (feed != null)
            {
                foreach (var package in feed.Packages)
                {
                    var result = _feed.Packages.Where(p => p.Id == package && p.IsLatestVersion).SingleOrDefault();
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
            _feedRepository.InsertPackagesIntoFeed(currentUser, id);
            return "<span class=\"label notice\">Added</span>";
        }

        [HttpGet]
        [Authorize]
        public ActionResult AddPackagesToMyFeed(Guid id)
        {
            var uploadPackagesRequest = this._uploadPackagesRequestRepository.GetByToken(id);
            if (uploadPackagesRequest == null)
            {
                TempData["Message"] = "packages.config not found";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            if (uploadPackagesRequest.Packages.Length < 1)
            {
                TempData["Message"] = "No packages found in packages.config";
                TempData["MessageType"] = "warning";
                return RedirectToAction("Index");
            }

            var currentUser = _userRepository.GetByUsername(User.Identity.Name);
            _feedRepository.InsertPackagesIntoFeed(currentUser, uploadPackagesRequest.Packages);

            TempData["Message"] = string.Format(
                "{0} package{1} successfully added",
                uploadPackagesRequest.Packages.Length,
                uploadPackagesRequest.Packages.Length == 1 ? string.Empty : "s");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddPackagesToMyFeed([ModelBinder(typeof(XmlModelBinder))] XDocument packagesConfig)
        {
            if (packagesConfig == null)
            {
                return new HttpStatusCodeResult(400);
            }

            string[] packages;
            try
            {
                packages = packagesConfig
                    .Descendants("package")
                    .Select(descendant => descendant.Attribute("id").Value)
                    .ToArray();
            }
            catch
            {
                return new HttpStatusCodeResult(400);
            }

            var token = Guid.NewGuid();
            var request = new UploadPackagesRequest { Packages = packages, Token = token };
            _uploadPackagesRequestRepository.Insert(request);

            return new ContentResult { Content = token.ToString() };
        }
    }
}
