using OctopusReleaseRetention.Core;
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
        var tags = new HashSet<string> { "prod" };

        foreach (var project in _projectRepository.GetAll())
        {
            var taggedReleases = _releaseRepository.GetAll(r => r.ProjectId == project.Id && tags.Contains(r.Tag ?? ""));

            foreach (var release in taggedReleases)
            {
                releasesToRetain.Add(release);
                _logger.Log($"Release {release.Id} (Version: {release.Version}) was retained because it is tagged = '{release.Tag}'");
            }

            foreach (var deploymentEnvironment in _deploymentEnvironmentRepository.GetAll())
            {
                var nonTaggedDeployments = _deploymentRepository
                    .GetAll(d =>
                        _releaseRepository.GetById(d.ReleaseId)?.ProjectId == project.Id
                        && !tags.Contains(_releaseRepository.GetById(d.ReleaseId)?.Tag ?? string.Empty)
                        && d.EnvironmentId == deploymentEnvironment.Id);

                var topReleases = nonTaggedDeployments
                    .GroupBy(d => d.ReleaseId)
                    .Select(g => new { ReleaseId = g.Key, EnvironmentId = deploymentEnvironment.Id, DeployedAt = g.Max(d => d.DeployedAt) })
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
        }

        return releasesToRetain.ToList();
    }

    public async Task<List<Release>> GetReleasesToKeepAsync(int numberOfReleasesToKeep)
    {
        var projects = _projectRepository.GetAll();

        var tasks = projects.Select(p => GetProjectReleasesToKeepAsync(numberOfReleasesToKeep, p));

        var results = await Task.WhenAll(tasks);

        return results.SelectMany(r => r).Distinct().ToList();
    }

    private async Task<List<Release>> GetProjectReleasesToKeepAsync(int numberOfReleasesToKeep, Project project)
    {
        var releasesToRetain = new HashSet<Release>();

        await Task.Run(() => {

            var deploymentEnvironments = _deploymentEnvironmentRepository.GetAll();

            foreach (var environment in deploymentEnvironments)
            {
                var deploymentsByProjAndEnv = _deploymentRepository.GetAll(d => d.EnvironmentId == environment.Id
                        && _releaseRepository.GetById(d.ReleaseId)?.ProjectId == project.Id);

                var topReleases = deploymentsByProjAndEnv.GroupBy(d => d.ReleaseId)
                    .Select(g => new { ReleaseId = g.Key, EnvironmentId = environment.Id, DeployedAt = g.Max(d => d.DeployedAt) })
                    .OrderByDescending(r => r.DeployedAt)
                    .Take(numberOfReleasesToKeep)
                    .ToList();

                foreach (var topRelease in topReleases)
                {
                    var release = _releaseRepository.GetById(topRelease.ReleaseId)!;
                    releasesToRetain.Add(release);
                    _logger.Log($"Release {release.Id} (Version: {release.Version}) was retained because it is within {numberOfReleasesToKeep} most recent deployed in {topRelease.EnvironmentId}. Deployment date is: {topRelease.DeployedAt}");
                }
            }
        });
        return releasesToRetain.ToList();
    }
}
