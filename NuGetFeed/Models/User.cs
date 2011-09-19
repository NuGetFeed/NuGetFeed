using Norm;

namespace NuGetFeed.Models
{
    public class User
    {
        [MongoIdentifier]
        public ObjectId Id { get; set; }
        
        public string Username { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string Email { get; set; }
    }
}