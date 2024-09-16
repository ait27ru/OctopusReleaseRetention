namespace OctopusReleaseRetention.Interfaces;

public interface IRepository<T> where T : class
{
    List<T> GetAll(Func<T, bool>? filter = null);
    T? GetById(string id);
    void Add(T entity);
    void AddRange(IEnumerable<T> range);
}
