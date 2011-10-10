namespace NuGetFeed.NuGetService
{
    using System.Linq;

    using NuGetFeed.Infrastructure.PackageSources;

    public partial class GalleryFeedContext : IGalleryFeedContext
    {
        public IQueryable<PublishedPackage> AllPackages
        {
            get
            {
                return Packages;
            }
        }

        public IQueryable<PublishedScreenshot> AllScreenshots
        {
            get
            {
                return Screenshots;
            }
        }
    }
}