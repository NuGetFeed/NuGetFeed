using System;
using System.Collections.Generic;
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

        public PublishedPackage GetLatestVersion(string packageId)
        {
            return _context.Packages.Where(p => p.Id == packageId && p.IsLatestVersion).SingleOrDefault();
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
    }
}