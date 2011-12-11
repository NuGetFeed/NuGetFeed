using System.Collections.ObjectModel;
using NuGetFeed.NuGetService;

namespace NuGetFeed.ViewModels
{
    public class PackageViewModel
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public string Title { get; set; }

        public string Authors { get; set; }

        public string Summary { get; set; }

        public string Description { get; set; }

        public string IconUrl { get; set; }

        public string ProjectUrl { get; set; }
    }
}