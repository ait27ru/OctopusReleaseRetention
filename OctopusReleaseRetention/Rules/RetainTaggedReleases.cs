using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.Rules;

public class RetainTaggedReleases : IRetentionRule
{
    private readonly HashSet<string> _tagsToRetain;

    public RetainTaggedReleases(IEnumerable<string> tagsToRetain)
    {
        _tagsToRetain = new HashSet<string>(tagsToRetain);
    }

    public bool ShouldRetain(Project project, Release release)
    {
        if (release?.Tags == null)
        {
            return false;
        }
        return _tagsToRetain.Overlaps(release.Tags);
    }
}
