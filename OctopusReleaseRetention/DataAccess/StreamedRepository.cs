using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.DataAccess;

public class StreamedRepository<T> : IRepository<T> where T : class
{
    private readonly IEnumerable<T> _entities;

    public StreamedRepository(IEnumerable<T> entities)
    {
        _entities = entities;
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
        throw new NotImplementedException();
    }

    public void AddRange(IEnumerable<T> range)
    {
        throw new NotImplementedException();
    }
}
