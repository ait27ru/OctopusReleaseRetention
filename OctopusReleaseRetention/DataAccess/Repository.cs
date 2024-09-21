using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.DataAccess;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly List<T> _entities = new List<T>();

    public Repository(IEnumerable<T> entities)
    {
        _entities = entities.ToList();
    }

    public IEnumerable<T> GetAll(Func<T, bool>? filter = null)
    {
        if (filter != null)
        {
            return _entities.Where(filter);
        }
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
