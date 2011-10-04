using System;
using System.Collections.Generic;
using System.Linq;
using Norm;
using Norm.Collections;
using Norm.Responses;
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

        public long GetNumberOfFeeds()
        {
            return _feeds.Count();
        }

        public IEnumerable<PackageCount> MostPopular()
        {
            string map = @"function() { 
                                this.Packages.forEach(
                                    function(z) {
                                        emit(z, 1);
                                    }
                                );
                            }";

            string reduce = @"function(key, values) {
                                var total = 0;
                                for(var i = 0; i < values.length; i++)
                                {
                                    total += values[i];
                                }
                                return total;
                            }";

            MapReduce mr = _mongo.Database.CreateMapReduce();
            MapReduceOptions options = new MapReduceOptions<Feed>
                                           {
                                               Map = map,
                                               Reduce = reduce,
                                               OutputCollectionName = "PackageTimesInFeed",
                                               Permanant = false
                                           };
            MapReduceResponse response = null;
            try
            {
                response = mr.Execute(options);
            }
            catch (Exception)
            {
                // MongoDB 2.0.0 seems to return Reduce field which NoRM can not parse :(
            }

            var packageCount = _mongo.Database.GetCollection<PackageCount>("PackageTimesInFeed").AsQueryable();
            return packageCount.OrderByDescending(x => x.value).Take(25);
        }
    }
}