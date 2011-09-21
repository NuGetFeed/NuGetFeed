using System.Collections.Generic;
using NuGetFeed.NuGetService;

namespace NuGetFeed.ViewModels
{
    public class MyFeedViewModel
    {
        public string FeedId { get; set; }

        public IList<PublishedPackage> Packages { get; set; }
    }
}