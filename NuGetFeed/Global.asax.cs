using System.Collections.Generic;
using System.Configuration;
using System.Web.Mvc;
using System.Web.Routing;
using Canonicalize;
using DotNetOpenAuth.GoogleOAuth2;
using LowercaseRoutesMVC;
using Microsoft.Web.WebPages.OAuth;

namespace NuGetFeed
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRouteLowercase(
                "Package details",
                "List/Packages/{id}/Details",
                new { controller = "Packages", action = "Details" });

            routes.MapRouteLowercase(
                "Category",
                "Category/{id}",
                new { controller = "Category", action = "Index" });

            routes.MapRouteLowercase(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional });
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            ConfigureAuth();
        }

        private void ConfigureAuth()
        {
            var googleClient = new GoogleOAuth2Client(ConfigurationManager.AppSettings["Google.ClientId"], 
                ConfigurationManager.AppSettings["Google.ClientSecret"]);
            var extradata = new Dictionary<string, object>();
            OAuthWebSecurity.RegisterClient(googleClient, "Google", extradata);
        }
    }
}