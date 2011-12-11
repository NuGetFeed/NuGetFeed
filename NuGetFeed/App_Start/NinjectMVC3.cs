using System.Configuration;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Mvc;
using Norm;
using NuGetFeed.Infrastructure.PackageSources;
using NuGetFeed.Infrastructure.Repositories;
using NuGetFeed.Infrastructure.Rss;
using NuGetFeed.Models;

[assembly: WebActivator.PreApplicationStartMethod(typeof(NuGetFeed.App_Start.NinjectMVC3), "Start")]
[assembly: WebActivator.ApplicationShutdownMethodAttribute(typeof(NuGetFeed.App_Start.NinjectMVC3), "Stop")]

namespace NuGetFeed.App_Start
{
    using System;

    using NuGetFeed.NuGetService;

    public static class NinjectMVC3 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestModule));
            DynamicModuleUtility.RegisterModule(typeof(HttpApplicationInitializationModule));
            bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // Register Mongo
            var mongoUrl = ConfigurationManager.AppSettings["MONGOHQ_URL"];
            kernel.Bind<IMongo>()
                .ToMethod(context => Mongo.Create(mongoUrl, "strict=false")) // strict=false makes null ref exceptions on deserialization go away...
                .InRequestScope();

            // Register Mongo repositories
            kernel.Bind<IRepository<User>>().To<UserRepository>().InRequestScope();
            kernel.Bind<IRepository<Feed>>().To<FeedRepository>().InRequestScope();

            // Register OData feeds
            kernel
                .Bind<IGalleryFeedContext>()
                .To<FeedContext_x0060_1>()
                .WithConstructorArgument("serviceRoot", new Uri("http://nuget.org/api/v2/"));
            kernel.Bind<NuGetOrgFeed>().ToSelf().InRequestScope();

            // Register helpers
            kernel.Bind<SyndicationHelper>().ToSelf();
        }
    }
}
