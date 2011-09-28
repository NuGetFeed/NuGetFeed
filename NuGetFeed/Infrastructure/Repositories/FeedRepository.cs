using System.Linq;
using Norm;
using Norm.Collections;
using NuGetFeed.Models;

namespace NuGetFeed.Infrastructure.Repositories
{
    public class FeedRepository : IRepository<Feed>
    {
        private readonly IMongo _mongo;
        private IMongoCollection<Feed> _feeds;

        public FeedRepository(IMongo mongo)
        {
            _mongo = mongo;
            _feeds = mongo.GetCollection<Feed>();
        }

        public void Insert(Feed obj)
        {
            _feeds.Insert(obj);
        }

        public void InsertPackagesIntoFeed(User currentUser, params string[] packageIds)
        {
            var feed = GetByUser(currentUser);
            if (feed == null)
            {
                feed = new Feed
                {
                    User = currentUser.Id
                };
            }

            foreach (var packageId in packageIds)
            {
                if (!feed.Packages.Contains(packageId.ToLowerInvariant()))
                {
                    feed.Packages.Add(packageId.ToLowerInvariant());
                }
            }

            Save(feed);
        }

        public void Save(Feed obj)
        {
            _feeds.Save(obj);
        }

        public Feed GetById(ObjectId id)
        {
            return _feeds.AsQueryable().Single(x => x.Id == id);
        }

        public Feed GetByUser(User user)
        {
            return _feeds.AsQueryable().SingleOrDefault(f => f.User == user.Id);
        }
    }
}