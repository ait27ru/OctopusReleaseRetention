using Newtonsoft.Json;
using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.DataAccess;

public class StreamedDataLoader : IDataLoader
{
    private readonly string _dataPath;
    private readonly ILogger _logger;

    public StreamedDataLoader(string dataPath, ILogger logger)
    {
        _dataPath = dataPath;
        _logger = logger;
    }

    public IEnumerable<T> LoadData<T>(string file) where T : class
    {
        var fileName = Path.Combine(_dataPath, file);

        if (!File.Exists(fileName))
        {
            _logger.Log($"File {fileName} not found.");
            throw new FileNotFoundException();
        }

        using var reader = new StreamReader(fileName);
        using var jsonReader = new JsonTextReader(reader);
        var serializer = new JsonSerializer();
        while (jsonReader.Read())
        {
            if (jsonReader.TokenType == JsonToken.StartObject)
            {
                yield return serializer.Deserialize<T>(jsonReader)
                    ?? throw new InvalidDataException();
            }
        }
    }
}