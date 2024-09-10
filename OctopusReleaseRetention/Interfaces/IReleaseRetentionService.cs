using OctopusReleaseRetention.Entities;

namespace OctopusReleaseRetention.Interfaces;

public interface IReleaseRetentionService
{
    List<Release> GetReleasesToKeep(int numberOfReleasesToKeep);

    List<Release> GetReleasesToKeep(int numberOfReleasesToKeep, IEnumerable<string> tagsToRetain);
}
