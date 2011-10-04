using NuGetFeed.NuGetService;

namespace NuGetFeed.ViewModels
{
    public class MostPopularViewModel
    {
        public PublishedPackage Package { get; set; }

        public decimal IncludedInFeeds { get; set; }
    }
}