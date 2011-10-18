namespace NuGetFeed.Models
{
    using System.Collections.Generic;

    using Norm;

    public class Category
    {
        [MongoIdentifier]
        public ObjectId Id { get; set; }

        public string Name { get; set; }

        public string VisibleName { get; set; }

        public string Description { get; set; }

        public string SourceUrl { get; set; }

        public List<string> Packages { get; protected set; }

        public Category()
        {
            Packages = new List<string>();
        }
    }
}