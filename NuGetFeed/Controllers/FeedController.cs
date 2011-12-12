using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Xml.Linq;
using Norm;
using NuGetFeed.Infrastructure.ActionResults;
using NuGetFeed.Infrastructure.ModelBinders;
using NuGetFeed.Infrastructure.PackageSources;
using NuGetFeed.Infrastructure.Repositories;
using NuGetFeed.Infrastructure.Rss;
using NuGetFeed.Models;
using NuGetFeed.NuGetService;
using NuGetFeed.ViewModels;

namespace NuGetFeed.Controllers
{
    using System.Web;

    public class FeedController : Controller
    {
        private readonly UserRepository _userRepository;
        private readonly FeedRepository _feedRepository;
        private readonly ListController _listController;
        private readonly UploadPackagesRequestRepository _uploadPackagesRequestRepository;
        private readonly NuGetOrgFeed _nuGetOrgFeed;
        private readonly SyndicationHelper _syndicationHelper;

        public FeedController(UserRepository userRepository, FeedRepository feedRepository, ListController listController, UploadPackagesRequestRepository uploadPackagesRequestRepository, NuGetOrgFeed nuGetOrgFeed, SyndicationHelper syndicationHelper)
        {
            _userRepository = userRepository;
            _feedRepository = feedRepository;
            _listController = listController;
            _uploadPackagesRequestRepository = uploadPackagesRequestRepository;
            _nuGetOrgFeed = nuGetOrgFeed;
            _syndicationHelper = syndicationHelper;
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

            var packages = new List<V1FeedPackage>();
            if (feed != null)
            {
                foreach (var package in feed.Packages)
                {
                    var result = _nuGetOrgFeed.GetLatestVersion(package);
                    if (result != null)
                    {
                        packages.Add(result);
                    }
                }
            }

            var viewModel = new MyFeedViewModel
                                {
                                    FeedId = feed != null ? feed.Id.ToString() : string.Empty,
                                    Packages = packages.OrderBy(x => x.Title).ToList()
                                };

            return View(viewModel);
        }

        public ActionResult Rss(string id)
        {
            var feed = _feedRepository.GetById(new ObjectId(id));
            var packages = _nuGetOrgFeed.GetPackagesFromList(feed.Packages);
            var items = packages.Select(p => _syndicationHelper.CreateNuGetPackageSyndicationItem(p)).ToList();
            var syndicationFeed = _syndicationHelper.CreateFeed(
                "NuGetFeed.org - My Feed",
                "Releases of NuGet packages included in your feed",
                id,
                items);

            return new RssActionResult { Feed = syndicationFeed };
        }

        public ActionResult PublicFeeds()
        {
            return View();
        }

        public ActionResult PublicRssRecentReleasesByAuthor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpNotFoundResult("Author cannot be null or empty");
            }

            var packages = _nuGetOrgFeed.GetByAuthor(id);
            var items = packages.Select(p => _syndicationHelper.CreateNuGetPackageSyndicationItem(p)).ToList();
            var feed = _syndicationHelper.CreateFeed(
                "NuGetFeed.org - Recent Releases by " + id,
                "Most recent package releases by " + id,
                "nugetfeedby" + id.Replace(" ", string.Empty),
                items);

            return new RssActionResult { Feed = feed };
        }

        public ActionResult PublicRssRecentReleases()
        {
            var packages = _nuGetOrgFeed.GetAllByDescendingPublishDate();
            var items = packages.Select(p => _syndicationHelper.CreateNuGetPackageSyndicationItem(p)).ToList();
            var feed = _syndicationHelper.CreateFeed(
                "NuGetFeed.org - Recent Releases",
                "Most recent package releases from NuGet",
                "nugetfeedorgmostrecent",
                items);

            return new RssActionResult { Feed = feed };
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
        public ActionResult AddToMyFeed(string id)
        {
            var currentUser = _userRepository.GetByUsername(User.Identity.Name);
            _feedRepository.InsertPackagesIntoFeed(currentUser, id);
            if (Request.IsAjaxRequest())
            {
                return Content("<span class=\"label notice\">Added</span>");
            }

            TempData["Message"] = id + " successfully added";
            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize]
        public ActionResult AddPackagesToMyFeed(Guid id)
        {
            var uploadPackagesRequest = _uploadPackagesRequestRepository.GetByToken(id);
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

        [HttpPost]
        [Authorize]
        public ActionResult AddPackagesToMyFeedPost(HttpPostedFileBase packagesFile)
        {
            if (packagesFile == null || (!packagesFile.ContentType.Equals("text/xml") && !packagesFile.ContentType.Equals("application/xml")))
            {
                TempData["Message"] = "No file selected or unknown filetype";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Feed");
            }

            var packagesXml = XDocument.Load(packagesFile.InputStream);
            string[] packages;
            try
            {
                packages = packagesXml
                    .Descendants("package")
                    .Select(descendant => descendant.Attribute("id").Value)
                    .ToArray();
            }
            catch
            {
                TempData["Message"] = "Error parsing specified packages.config";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index", "Feed");
            }

            var currentUser = _userRepository.GetByUsername(User.Identity.Name);
            _feedRepository.InsertPackagesIntoFeed(currentUser, packages);

            TempData["Message"] = string.Format(
                "{0} package{1} successfully added",
                packages.Length,
                packages.Length == 1 ? string.Empty : "s");

            return RedirectToAction("Index", "Feed");
        }

        public ActionResult SearchAuthors(string term)
        {
            return Json(_nuGetOrgFeed.SearchAuthors(term).Select(x => new { id = x, label = x, value = x }), JsonRequestBehavior.AllowGet);
        }
    }
}
