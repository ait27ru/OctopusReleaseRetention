using OctopusReleaseRetention.Entities;

namespace OctopusReleaseRetention.Interfaces;

public interface IRetentionRule
{
    public bool ShouldRetain(Project project, Release release);
}
