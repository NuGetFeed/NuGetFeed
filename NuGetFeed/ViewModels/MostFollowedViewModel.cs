using NuGetFeed.NuGetService;

namespace NuGetFeed.ViewModels
{
    public class MostFollowedViewModel
    {
        public PublishedPackage Package { get; set; }

        public decimal IncludedInFeeds { get; set; }
    }
}