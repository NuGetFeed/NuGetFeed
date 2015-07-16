using System.Linq;

using NuGetFeed.Infrastructure.PackageSources;

namespace NuGetFeed.NuGetService
{
    public partial class V1FeedContext : IGalleryFeedContext
    {
        public IQueryable<V1FeedPackage> AllPackages
        {
            get
            {
                return Packages;
            }
        }
    }
}