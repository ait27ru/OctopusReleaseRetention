using OctopusReleaseRetention.Entities;

namespace OctopusReleaseRetention.Interfaces;

public interface IReleaseRetentionService
{
    List<Release> GetReleasesToKeep(int numberOfReleasesToKeep);
    Task<List<Release>> GetReleasesToKeepAsync(int numberOfReleasesToKeep);
}
