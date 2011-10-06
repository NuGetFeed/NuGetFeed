using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NuGetFeed.NuGetService;

namespace NuGetFeed.Infrastructure.PackageSources
{
    public class NuGetOrgFeed
    {
        private readonly GalleryFeedContext _context;

        public NuGetOrgFeed()
        {
            _context = new GalleryFeedContext(new Uri("http://packages.nuget.org/v1/FeedService.svc/"));
        }

        public PublishedPackage GetLatestVersion(string packageId, bool includeScreenshots = false)
        {
            var package = _context.Packages.Where(p => p.Id == packageId && p.IsLatestVersion).SingleOrDefault();
            if (package != null && includeScreenshots)
            {
                var screenshots = _context.Screenshots
                    .Where(p => p.PublishedPackageId == package.Id && p.PublishedPackageVersion == package.Version)
                    .ToList();
                package.Screenshots = new Collection<PublishedScreenshot>(screenshots);
            }

            return package;
        }

        public IEnumerable<PublishedPackage> GetListOfPackageVersions(string packageId, int size)
        {
            var packages = (from p in _context.Packages
                            where p.Id == packageId
                            orderby p.LastUpdated descending
                            select p).Take(size);
            return packages;
        }

        public IEnumerable<PublishedPackage> Search(string query, int startFrom, int pageSize, out int numberOfResults)
        {
            var packages = _context.Packages.Where(p => (p.Id.Contains(query)
                                          || p.Description.Contains(query)
                                          || p.Tags.Contains(query)
                                          || p.Title.Contains(query))
                                         && p.IsLatestVersion);
            
            numberOfResults = packages.Count();
            return packages.OrderByDescending(x => x.DownloadCount).Skip(startFrom).Take(pageSize);
        }

        public IEnumerable<PublishedPackage> GetAllByDescendingPublishDate()
        {
            var packages = _context.Packages
                .Where(p => p.Id != "SymbolSource.TestPackage")
                .OrderByDescending(p => p.Published);
            return packages;
        }

        public IEnumerable<PublishedPackage> GetByAuthor(string author)
        {
            var packages =
                _context
                    .Packages
                    .Where(p => p.Authors.ToLower().Contains(author.ToLower()))
                    .OrderByDescending(p => p.Published);
            return packages;
        }

        public IEnumerable<string> SearchAuthors(string search)
        {
            var authors = this._context
                .Packages
                .Where(p => p.Authors.ToLower().Contains(search.ToLower()))
                .OrderByDescending(p => p.Published)
                .Select(x => new { x.Authors }) // Select(x => x.Authors) doesn't work (?)
                .ToList() // ToList, cause the following statements wont parse by the odata feed.
                .SelectMany(x => x.Authors.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                .Where(x => x.ToLower().Contains(search.ToLower()))
                .Select(x => x.ToLower().Trim())
                .Distinct();

            return authors;
        }
    }
}