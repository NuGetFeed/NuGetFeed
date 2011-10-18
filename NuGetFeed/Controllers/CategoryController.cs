using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NuGetFeed.Controllers
{
    using NuGetFeed.Infrastructure.AutoMapper;
    using NuGetFeed.Infrastructure.PackageSources;
    using NuGetFeed.Infrastructure.Repositories;
    using NuGetFeed.ViewModels;

    public class CategoryController : Controller
    {
        private readonly CategoryRepository _categoryRepository;

        private readonly NuGetOrgFeed _nuGetOrgFeed;

        public CategoryController(CategoryRepository categoryRepository, NuGetOrgFeed nuGetOrgFeed)
        {
            _categoryRepository = categoryRepository;
            _nuGetOrgFeed = nuGetOrgFeed;
        }

        public ActionResult Index(string id)
        {
            var category = _categoryRepository.GetByName(id);
            var categoryViewModel = category.MapToDynamic<CategoryViewModel>();
            foreach (var publishedPackage in category.Packages.Select(package => this._nuGetOrgFeed.GetLatestVersion(package)).Where(publishedPackage => publishedPackage != null))
            {
                categoryViewModel.Packages.Add(publishedPackage.MapToDynamic<PackageViewModel>());
            }

            return View(categoryViewModel);
        }
    }
}
