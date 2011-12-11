using NuGetFeed.NuGetService;

namespace NuGetFeed.ViewModels
{
    public class MostFollowedViewModel
    {
        public V2FeedPackage Package { get; set; }

        public decimal IncludedInFeeds { get; set; }
    }
}