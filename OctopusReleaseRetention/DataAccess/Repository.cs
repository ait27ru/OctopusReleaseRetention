using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.DataAccess;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly List<T> _entities = new List<T>();

    public Repository(List<T> entities)
    {
        _entities = entities;
    }

    public List<T> GetAll()
    {
        return _entities;
    }

    public T? GetById(string id)
    {
        return _entities.FirstOrDefault(e =>
            typeof(T).GetProperty("Id")?.GetValue(e)?.ToString() == id);
    }

    public void Add(T entity)
    {
        _entities.Add(entity);
    }

    public void AddRange(IEnumerable<T> range)
    {
        _entities.AddRange(range);
    }
}
