using System;
using System.Collections.Generic;
using System.Linq;

using NuGetFeed.NuGetService;

namespace NuGetFeed.Infrastructure.PackageSources
{
    public interface IGalleryFeedContext
    {
        IQueryable<V1FeedPackage> AllPackages { get; }

        IEnumerable<TElement> Execute<TElement>(Uri requestUri);
    }
}