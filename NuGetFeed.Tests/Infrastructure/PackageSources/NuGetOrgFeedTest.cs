using System.Linq;

using NuGetFeed.Infrastructure.PackageSources;
using NuGetFeed.NuGetService;

namespace NuGetFeed.Tests.Infrastructure.PackageSources
{
    using NUnit.Framework;

    public class NuGetOrgFeedTest : UnitTestBase
    {
        [Test]
        public void CanSearchAuthors()
        {
            // Arrange
            var author = A<string>();
            var mock = Mock<IGalleryFeedContext>();
            mock
                .Setup(x => x.AllPackages)
                .Returns(new[]
                    {
                        new V1FeedPackage { Authors = author }, // Author equals
                        new V1FeedPackage { Authors = A<string>() + "," + author + "," + A<string>() }, // Author contained
                        new V1FeedPackage { Authors = author + "," + A<string>() }, // Author first
                        new V1FeedPackage { Authors = A<string>() + "," + author }, // Author last
                        new V1FeedPackage { Authors = A<string>() }, // Author not contained
                    }.AsQueryable());

            var nuGetOrgFeed = new NuGetOrgFeed(mock.Object);

            // Act
            var result = nuGetOrgFeed.SearchAuthors(author);

            // Assert
            Assert.That(result != null);
            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First(), Is.EqualTo(author));
        }
    }
}