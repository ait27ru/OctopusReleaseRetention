using OctopusReleaseRetention.Interfaces;
using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.DataAccess;
using OctopusReleaseRetention.Integration.Tests;

namespace OctopusReleaseRetention.Services.Integration.Tests
{
    public class ReleaseRetentionServiceIntegrationTests
    {
        private readonly IReleaseRetentionService _sut;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<Release> _releaseRepository;
        private readonly IRepository<DeploymentEnvironment> _deploymentEnvironmentRepository;
        private readonly IRepository<Deployment> _deploymentRepository;
        private readonly ILogger _logger;

        public ReleaseRetentionServiceIntegrationTests()
        {
            _logger = new InMemoryLogger();
            var jsonLoader = new JsonDataLoader("TestData", _logger);

            _projectRepository = new IndexedRepository<Project>(jsonLoader.LoadDataAsync<Project>("Projects.json").Result);
            _releaseRepository = new IndexedRepository<Release>(jsonLoader.LoadDataAsync<Release>("Releases.json").Result);
            _deploymentEnvironmentRepository = new IndexedRepository<DeploymentEnvironment>(jsonLoader.LoadDataAsync<DeploymentEnvironment>("Environments.json").Result);
            _deploymentRepository = new IndexedRepository<Deployment>(jsonLoader.LoadDataAsync<Deployment>("Deployments.json").Result);

            _sut = new ReleaseRetentionService(_projectRepository, _deploymentEnvironmentRepository,
                _releaseRepository, _deploymentRepository, _logger);
        }

        public class GetReleasesToKeep : ReleaseRetentionServiceIntegrationTests
        {

            [Fact()]
            public void ShouldReturnAllDeployedReleases_WhenFewerDeployedReleasesExist_ThanRetentionLimit()
            {
                // Arrane
                var releasesToKeep = 5;

                // Act
                var result = _sut.GetReleasesToKeep(releasesToKeep);

                // Assert
                Assert.Equal(5, result.Count);
                Assert.Contains(result, r => r.Id == "Release-1");
                Assert.Contains(result, r => r.Id == "Release-2");
                Assert.Contains(result, r => r.Id == "Release-5");
                Assert.Contains(result, r => r.Id == "Release-6");
                Assert.Contains(result, r => r.Id == "Release-7");
            }

            [Fact()]
            public void ShouldLogAllDeployedReleases_WhenFewerDeployedReleasesExist_ThanRetentionLimit()
            {
                // Arrane
                var releasesToKeep = 5;

                // Act
                _sut.GetReleasesToKeep(releasesToKeep);

                // Assert
                var logs = ((InMemoryLogger)_logger).GetLogs();
                Assert.Equal(7, logs.Count);
                Assert.Contains(logs, m => m == "Release Release-2 (Version: 1.0.1) was retained because it is within 5 most recent deployed in Environment-1. Deployment date is: 2/01/2000 10:00:00 AM");
                Assert.Contains(logs, m => m == "Release Release-1 (Version: 1.0.0) was retained because it is within 5 most recent deployed in Environment-1. Deployment date is: 1/01/2000 10:00:00 AM");
                Assert.Contains(logs, m => m == "Release Release-1 (Version: 1.0.0) was retained because it is within 5 most recent deployed in Environment-2. Deployment date is: 2/01/2000 11:00:00 AM");
                Assert.Contains(logs, m => m == "Release Release-6 (Version: 1.0.2) was retained because it is within 5 most recent deployed in Environment-1. Deployment date is: 2/01/2000 2:00:00 PM");
                Assert.Contains(logs, m => m == "Release Release-7 (Version: 1.0.3) was retained because it is within 5 most recent deployed in Environment-1. Deployment date is: 2/01/2000 1:00:00 PM");
                Assert.Contains(logs, m => m == "Release Release-5 (Version: 1.0.1-ci1) was retained because it is within 5 most recent deployed in Environment-1. Deployment date is: 1/01/2000 11:00:00 AM");
                Assert.Contains(logs, m => m == "Release Release-6 (Version: 1.0.2) was retained because it is within 5 most recent deployed in Environment-2. Deployment date is: 2/01/2000 11:00:00 AM");
            }

            [Fact()]
            public void ShouldReturnCorrectReleases_WhenMoreDeployedReleasesExist_ThanRetentionLimit()
            {
                // Arrane
                var releasesToKeep = 1;

                // Act
                var result = _sut.GetReleasesToKeep(releasesToKeep);

                // Assert
                Assert.Equal(3, result.Count);
                Assert.Contains(result, r => r.Id == "Release-1");
                Assert.Contains(result, r => r.Id == "Release-2");
                Assert.Contains(result, r => r.Id == "Release-6");
            }

            [Fact()]
            public void ShouldLogCorrectReleases_WhenMoreDeployedReleasesExist_ThanRetentionLimit()
            {
                // Arrane
                var releasesToKeep = 1;

                // Act
                _sut.GetReleasesToKeep(releasesToKeep);

                // Assert
                var logs = ((InMemoryLogger)_logger).GetLogs();
                Assert.Equal(4, logs.Count);
                Assert.Contains(logs, m => m == "Release Release-2 (Version: 1.0.1) was retained because it is within 1 most recent deployed in Environment-1. Deployment date is: 2/01/2000 10:00:00 AM");
                Assert.Contains(logs, m => m == "Release Release-1 (Version: 1.0.0) was retained because it is within 1 most recent deployed in Environment-2. Deployment date is: 2/01/2000 11:00:00 AM");
                Assert.Contains(logs, m => m == "Release Release-6 (Version: 1.0.2) was retained because it is within 1 most recent deployed in Environment-1. Deployment date is: 2/01/2000 2:00:00 PM");
                Assert.Contains(logs, m => m == "Release Release-6 (Version: 1.0.2) was retained because it is within 1 most recent deployed in Environment-2. Deployment date is: 2/01/2000 11:00:00 AM");
            }
        }
    }
}