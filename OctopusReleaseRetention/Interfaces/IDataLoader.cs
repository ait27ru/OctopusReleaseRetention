namespace OctopusReleaseRetention.Interfaces
{
    public interface IDataLoader
    {
        List<T> LoadData<T>(string fileName) where T : class;
    }
}