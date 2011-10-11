namespace NuGetFeed.ViewModels
{
    using System.Collections.Generic;

    public class CategoryViewModel
    {
        public CategoryViewModel()
        {
            this.PackageViewModels = new List<PackageViewModel>();
        }

        public string VisibleName { get; set; }

        public string Description { get; set; }

        public string SourceUrl { get; set; }

        public string IconUrl { get; set; }

        public List<PackageViewModel> PackageViewModels { get; protected set; }
    }
}