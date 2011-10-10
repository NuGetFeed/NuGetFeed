using System.Data.Services.Client;

using NuGetFeed.NuGetService;

namespace NuGetFeed.Infrastructure.PackageSources
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IGalleryFeedContext
    {
        IQueryable<PublishedPackage> AllPackages { get; }

        IQueryable<PublishedScreenshot> AllScreenshots { get; }

        IEnumerable<TElement> Execute<TElement>(Uri requestUri);
    }
}