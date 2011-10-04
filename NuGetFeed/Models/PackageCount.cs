using Norm;

namespace NuGetFeed.Models
{
    public class PackageCount
    {
        [MongoIdentifier]
        public string Id { get; set; }

        // This has to be lowercase value or else Mongo/NoRM can not order by this property
        public int value { get; set; }
    }
}