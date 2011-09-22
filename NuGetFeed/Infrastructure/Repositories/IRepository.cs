namespace NuGetFeed.Infrastructure.Repositories
{
    public interface IRepository<T>
    {
        void Insert(T obj);

        void Save(T obj);
    }
}