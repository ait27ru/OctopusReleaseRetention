namespace OctopusReleaseRetention.Interfaces
{
    public interface IDataLoader
    {
        IEnumerable<T> LoadData<T>(string fileName) where T : class;
    }
}