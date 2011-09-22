using System.Linq;
using Norm;
using Norm.Collections;
using NuGetFeed.Models;

namespace NuGetFeed.Infrastructure.Repositories
{
    public class UserRepository : IRepository<User>
    {
        private readonly IMongo _mongo;
        private IMongoCollection<User> _users;

        public UserRepository(IMongo mongo)
        {
            _mongo = mongo;
            _users = mongo.GetCollection<User>();
        }

        public void Insert(User user)
        {
            _users.Insert(user);
        }

        public void Save(User obj)
        {
            _users.Save(obj);
        }

        public User GetByUsername(string username)
        {
            return _users.AsQueryable().SingleOrDefault(u => u.Username == username);
        }
    }
}