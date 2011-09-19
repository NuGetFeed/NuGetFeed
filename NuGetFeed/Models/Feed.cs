using System.Collections.Generic;
using Norm;

namespace NuGetFeed.Models
{
    public class Feed
    {
        [MongoIdentifier]
        public ObjectId Id { get; set; }

        public ObjectId User { get; set; }

        public List<string> Packages { get; protected set; }

        public Feed()
        {
            Packages = new List<string>();
        }
    }
}