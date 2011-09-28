using System;
using System.Xml.Linq;

using Norm;

namespace NuGetFeed.Models
{
    public class UploadPackagesRequest
    {
        [MongoIdentifier]
        public ObjectId Id { get; set; }

        public Guid Token { get; set; }

        public string[] Packages { get; set; }
    }
}