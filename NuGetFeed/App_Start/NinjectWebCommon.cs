using System.Configuration;
using Norm;
using NuGetFeed.Infrastructure.PackageSources;
using NuGetFeed.Infrastructure.Repositories;
using NuGetFeed.Infrastructure.Rss;
using NuGetFeed.Models;
using NuGetFeed.NuGetService;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NuGetFeed.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NuGetFeed.App_Start.NinjectWebCommon), "Stop")]

namespace NuGetFeed.App_Start
{
    using System;
    using System.Web;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

    using Ninject;
    using Ninject.Web.Common;

    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
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
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
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
                .To<V1FeedContext>()
                .WithConstructorArgument("serviceRoot", new Uri("http://nuget.org/api/v1/"));
            kernel.Bind<NuGetOrgFeed>().ToSelf().InRequestScope();

            // Register helpers
            kernel.Bind<SyndicationHelper>().ToSelf();

        }        
    }
}
