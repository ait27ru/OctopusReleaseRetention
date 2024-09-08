using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.Integration.Tests;

public class InMemoryLogger : ILogger
{
    private readonly List<string> _logs = new List<string>();
    public void Log(string message)
    {
        _logs.Add(message);
    }
    public List<string> GetLogs() { return _logs; }
}
