using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;
using OctopusReleaseRetention.Rules;

namespace OctopusReleaseRetention.Policies;

public class RetainHighPriorityProjectsOrTaggedImportant : IRetentionPolicy
{
    private readonly IRetentionRule _highPriorityRule = new ProjectPriorityRule(Core.Priority.High);
    private readonly IRetentionRule _taggedImportantRule = new RetainTaggedReleases(new HashSet<string> { "important" });
    public bool ShouldRetain(Project project, Release release)
    {
        return _highPriorityRule.ShouldRetain(project, release) || _taggedImportantRule.ShouldRetain(project, release);
    }
}
