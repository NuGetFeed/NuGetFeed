using System.Collections.Generic;
using NuGetFeed.NuGetService;

namespace NuGetFeed.ViewModels
{
    public class PackageSearchViewModel
    {
        public string Query { get; set; }

        public IList<PublishedPackage> Packages { get; set; }
    }
}