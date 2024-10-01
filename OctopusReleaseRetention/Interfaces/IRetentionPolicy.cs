using OctopusReleaseRetention.Entities;

namespace OctopusReleaseRetention.Interfaces;

public interface IRetentionPolicy
{
    public bool ShouldRetain(Project project, Release release);
}