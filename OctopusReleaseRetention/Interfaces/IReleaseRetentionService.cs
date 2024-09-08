using OctopusReleaseRetention.Entities;

namespace OctopusReleaseRetention.Interfaces;

public interface IReleaseRetentionService
{
    List<Release> GetReleasesToKeep(int numberOfReleasesToKeep);
}
