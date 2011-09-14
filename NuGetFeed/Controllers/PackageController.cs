using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NuGetFeed.NuGetService;

namespace NuGetFeed.Controllers
{
    public class PackageController : Controller
    {
        //
        // GET: /Package/

        public ActionResult Index()
        {
            var service = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));
            var e = from o in service.Packages
                    select o;
            var b = e.Take(10).ToList();

            return new EmptyResult();
        }

    }
}
