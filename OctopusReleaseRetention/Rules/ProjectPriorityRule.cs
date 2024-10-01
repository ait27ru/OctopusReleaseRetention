using OctopusReleaseRetention.Core;
using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.Rules;

public class ProjectPriorityRule : IRetentionRule
{
    private readonly Priority _priority;

    public ProjectPriorityRule(Priority priority)
    {
        _priority = priority;
    }

    public bool ShouldRetain(Project project, Release release)
    {
        return project.Priority == _priority;
    }
}
