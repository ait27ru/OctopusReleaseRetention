using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;
using System;

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
        var releasesToRetain = new HashSet<Release>();

        var deploymentsByProjectAndEnvironment = _deploymentRepository.GetAll()
            .GroupBy(d => new { d.EnvironmentId, _releaseRepository.GetById(d.ReleaseId)!.ProjectId })
            .ToList();

        foreach (var group in deploymentsByProjectAndEnvironment)
        {
            var topReleases = group
                .GroupBy(d => d.ReleaseId)
                .Select(g => new { ReleaseId = g.Key, DeployedAt = g.Max(d => d.DeployedAt), group.Key.EnvironmentId })
                .OrderByDescending(d => d.DeployedAt)
                .Take(numberOfReleasesToKeep)
                .ToList();

            foreach (var topRelease in topReleases)
            {
                var release = _releaseRepository.GetById(topRelease.ReleaseId)!;
                releasesToRetain.Add(release);
                _logger.Log($"Release {release.Id} (Version: {release.Version}) was retained because it is within {numberOfReleasesToKeep} most recent deployed in {topRelease.EnvironmentId}. Deployment date is: {topRelease.DeployedAt}");
            }
        }
        return releasesToRetain.ToList();
    }
}
