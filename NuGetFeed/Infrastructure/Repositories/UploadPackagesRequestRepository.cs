using System;
using System.Linq;
using Norm;
using Norm.Collections;
using NuGetFeed.Models;

namespace NuGetFeed.Infrastructure.Repositories
{
    public class UploadPackagesRequestRepository : IRepository<UploadPackagesRequest>
    {
        private readonly IMongoCollection<UploadPackagesRequest> _uploadRequests;

        public UploadPackagesRequestRepository(IMongo mongo)
        {
            _uploadRequests = mongo.GetCollection<UploadPackagesRequest>();
        }

        public void Insert(UploadPackagesRequest obj)
        {
            _uploadRequests.Insert(obj);
        }

        public void Save(UploadPackagesRequest obj)
        {
            _uploadRequests.Save(obj);
        }

        public UploadPackagesRequest GetByToken(Guid token)
        {
            return _uploadRequests.AsQueryable().SingleOrDefault(u => u.Token == token);
        }
    }
}