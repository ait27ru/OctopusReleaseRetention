using OctopusReleaseRetention.Interfaces;
using System.Collections.Concurrent;

namespace OctopusReleaseRetention.DataAccess;

public class IndexedRepository<T> : IRepository<T> where T : class
{
    private readonly ConcurrentDictionary<string, T> _entities = new();

    public IndexedRepository(List<T> entities)
    {
        foreach (var entity in entities)
        {
            var id = GetId(entity);
            _entities[id] = entity;
        }
    }

    public IEnumerable<T> GetAll(Func<T, bool>? filter = null)
    {
        if (filter != null)
        {
            return _entities.Values.Where(filter);
        }
        return _entities.Values;
    }

    public T? GetById(string id)
    {
        if (!_entities.ContainsKey(id))
        {
            return null;
        }

        return _entities[id];
    }

    public void Add(T entity)
    {
        var id = GetId(entity);
        _entities[id] = entity;
    }

    public void AddRange(IEnumerable<T> range)
    {
        foreach (var item in range)
        {
            Add(item);
        }
    }

    private string GetId(T entity)
    {
        return typeof(T).GetProperty("Id")?.GetValue(entity)?.ToString()
            ?? throw new ArgumentOutOfRangeException("Id is null");
    }
}
