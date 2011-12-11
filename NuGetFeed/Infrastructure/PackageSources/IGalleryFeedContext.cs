using NuGetFeed.NuGetService;

namespace NuGetFeed.Infrastructure.PackageSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IGalleryFeedContext
    {
        IQueryable<V2FeedPackage> AllPackages { get; }

        IEnumerable<TElement> Execute<TElement>(Uri requestUri);
    }
}