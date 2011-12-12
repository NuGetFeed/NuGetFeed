using System.Collections.Generic;
using NuGetFeed.NuGetService;

namespace NuGetFeed.ViewModels
{
    public class PackageSearchViewModel
    {
        public string Query { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public IList<V1FeedPackage> Packages { get; set; }
    }
}