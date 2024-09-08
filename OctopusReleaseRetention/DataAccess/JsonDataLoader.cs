using OctopusReleaseRetention.Interfaces;
using System.Text.Json;

namespace OctopusReleaseRetention.DataAccess;

public class JsonDataLoader : IDataLoader
{
    private readonly string _dataPath;
    private readonly ILogger _logger;

    public JsonDataLoader(string dataPath, ILogger logger)
    {
        _dataPath = dataPath;
        _logger = logger;
    }

    public List<T> LoadData<T>(string file) where T : class
    {
        var fileName = Path.Combine(_dataPath, file);

        if (!File.Exists(fileName))
        {
            _logger.Log($"File {fileName} not found.");
            throw new FileNotFoundException();
        }

        var jsonData = File.ReadAllText(fileName);

        try
        {
            var data = JsonSerializer.Deserialize<List<T>>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data ?? new List<T>();
        }
        catch (Exception)
        {
            _logger.Log($"Error deserialising from {fileName}.");
            throw;
        }
    }
}
