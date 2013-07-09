using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using NuGetFeed.NuGetService;

namespace NuGetFeed.Infrastructure.PackageSources
{
    public class NuGetOrgFeed
    {
        private readonly IGalleryFeedContext _context;

        public NuGetOrgFeed(IGalleryFeedContext context)
        {
            _context = context;
        }

        public V1FeedPackage GetLatestVersion(string packageId)
        {
            var package = _context.AllPackages.Where(p => p.Id == packageId && p.IsLatestVersion).SingleOrDefault();
            return package;
        }

        public IEnumerable<V1FeedPackage> GetListOfPackageVersions(string packageId, int size)
        {
            var packages = (from p in _context.AllPackages
                            where p.Id == packageId
                            orderby p.Published descending
                            select p).Take(size);
            return packages;
        }

        public IEnumerable<V1FeedPackage> Search(string query, int startFrom, int pageSize, out int numberOfResults)
        {
            var packages = _context.AllPackages.Where(p => (p.Id.Contains(query)
                                          || p.Description.Contains(query)
                                          || p.Tags.Contains(query)
                                          || p.Title.Contains(query))
                                         && p.IsLatestVersion);
            
            numberOfResults = packages.Count();
            return packages.OrderByDescending(x => x.DownloadCount).Skip(startFrom).Take(pageSize);
        }

        public IEnumerable<V1FeedPackage> GetAllByDescendingPublishDate()
        {
            var packages = _context.AllPackages
                .Where(p => p.Id != "SymbolSource.TestPackage")
                .OrderByDescending(p => p.Published);
            return packages;
        }

        public IEnumerable<V1FeedPackage> GetByAuthor(string author)
        {
            var packages =
                _context
                    .AllPackages
                    .Where(p => p.Authors.ToLower().Contains(author.ToLower()))
                    .OrderByDescending(p => p.Published);
            return packages;
        }

        public IEnumerable<string> SearchAuthors(string search)
        {
            var authors = this._context
                .AllPackages
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

        /// <summary>
        /// Optimized method for getting all PublishedPackages for a list of id's.
        /// </summary>
        public List<V1FeedPackage> GetPackagesFromList(List<string> packages)
        {
            var query = string.Empty;
            for (int i = 0; i < packages.Count; i++)
            {
                query += "Id eq '" + packages[i] + "'";
                if (i != packages.Count - 1)
                {
                    query += " or ";
                }
            }

            var request = _context.Execute<V1FeedPackage>(new Uri(
                                                          "Packages()?$filter=" + query +
                                                          "&$orderby=Published desc",
                                                          UriKind.RelativeOrAbsolute));

            return request.ToList();
        }
    }
}