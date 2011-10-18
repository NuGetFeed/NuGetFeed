namespace NuGetFeed.Infrastructure.Repositories
{
    using System.Linq;

    using Norm;
    using Norm.Collections;

    using NuGetFeed.Models;

    public class CategoryRepository : IRepository<Category>
    {
        private readonly IMongo _mongo;

        private IMongoCollection<Category> _categories;

        public CategoryRepository(IMongo mongo)
        {
            _mongo = mongo;
            _categories = mongo.GetCollection<Category>();
        }

        public void Insert(Category obj)
        {
            _categories.Insert(obj);
        }

        public void Save(Category obj)
        {
            _categories.Save(obj);
        }

        public Category GetByName(string name)
        {
            return _categories.AsQueryable().Single(x => x.Name == name);
        }
    }
}