using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.Services;

public class ReleaseRetentionService : IReleaseRetentionService
{
    private readonly IRepository<Project> _projectRepository;
    private readonly IRepository<DeploymentEnvironment> _deploymentEnvironmentRepository;
    private readonly IRepository<Release> _releaseRepository;
    private readonly IRepository<Deployment> _deploymentRepository;
    private readonly ILogger _logger;

    public ReleaseRetentionService(
        IRepository<Project> projectRepository,
        IRepository<DeploymentEnvironment> deploymentEnvironmentRepository,
        IRepository<Release> releaseRepository,
        IRepository<Deployment> deploymentRepository,
        ILogger logger)
    {
        _projectRepository = projectRepository;
        _deploymentEnvironmentRepository = deploymentEnvironmentRepository;
        _releaseRepository = releaseRepository;
        _deploymentRepository = deploymentRepository;
        _logger = logger;
    }

    public List<Release> GetReleasesToKeep(int numberOfReleasesToKeep)
    {
        return GetReleasesToKeep(numberOfReleasesToKeep, new List<string>());
    }

    public List<Release> GetReleasesToKeep(int numberOfReleasesToKeep, IEnumerable<string> tagsToRetain)
    {
        var releasesToRetain = new HashSet<Release>();
        var tagsHash = new HashSet<string>(tagsToRetain ?? new List<string>());

        var taggedReleases = _releaseRepository.GetAll(r => r.Tags?.Overlaps(tagsHash) ?? false);

        foreach (var release in taggedReleases)
        {
            releasesToRetain.Add(release);
            _logger.Log($"Release {release.Id} (Version: {release.Version}) was retained because it is tagged = '{string.Join(',', release?.Tags ?? [""])}'");
        }

        foreach (var project in _projectRepository.GetAll())
        {
            foreach (var deplEnv in _deploymentEnvironmentRepository.GetAll())
            {
                var nonTaggedDeployments = _deploymentRepository.GetAll(d =>
                {
                    var release = _releaseRepository.GetById(d.ReleaseId)!;
                    return d.EnvironmentId == deplEnv.Id
                        && release.ProjectId == project.Id
                        && !(release.Tags?.Overlaps(tagsHash) ?? false);
                });

                var topReleases = nonTaggedDeployments.GroupBy(r => r.ReleaseId)
                    .Select(g => new { ReleaseId = g.Key, EnvironmentId = deplEnv.Id, DeployedAt = g.Max(d => d.DeployedAt) })
                    .OrderByDescending(d => d.DeployedAt)
                    .Take(numberOfReleasesToKeep);

                foreach (var topRelease in topReleases)
                {
                    var release = _releaseRepository.GetById(topRelease.ReleaseId)!;
                    releasesToRetain.Add(release);
                    _logger.Log($"Release {release.Id} (Version: {release.Version}) was retained because it is within {numberOfReleasesToKeep} most recent deployed in {topRelease.EnvironmentId}. Deployment date is: {topRelease.DeployedAt}");
                }

            }
        }

        return releasesToRetain.ToList();
    }
}
