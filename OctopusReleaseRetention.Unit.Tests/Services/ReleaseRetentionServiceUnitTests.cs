using Moq;
using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;
using OctopusReleaseRetention.Services;

namespace OctopusReleaseRetention.Unit.Tests.Services;
public class ReleaseRetentionServiceUnitTests
{
    private readonly Mock<IRepository<Project>> _projectRepositoryMock;
    private readonly Mock<IRepository<DeploymentEnvironment>> _deploymentEnvironmentRepositoryMock;
    private readonly Mock<IRepository<Release>> _releaseRepositoryMock;
    private readonly Mock<IRepository<Deployment>> _deploymentRepositoryMock;
    private readonly Mock<ILogger> _loggerMock;

    private readonly ReleaseRetentionService _sut;

    public ReleaseRetentionServiceUnitTests()
    {
        _projectRepositoryMock = new();
        _deploymentEnvironmentRepositoryMock = new();
        _releaseRepositoryMock = new();
        _deploymentRepositoryMock = new();
        _loggerMock = new();

        _sut = new ReleaseRetentionService(_projectRepositoryMock.Object,
            _deploymentEnvironmentRepositoryMock.Object,
            _releaseRepositoryMock.Object,
            _deploymentRepositoryMock.Object,
            _loggerMock.Object);
    }

    public class GetReleasesToKeep : ReleaseRetentionServiceUnitTests
    {
        [Fact]
        public void ShouldReturnCorrectReleases_WhenMoreDeployedReleasesExist_ThanRetentionLimit()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);

            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", ReleaseId = releases[0].Id, EnvironmentId = deploymentEnvironments[0].Id, DeployedAt = DateTime.Now.AddDays(-5) },
                new Deployment { Id = "Deployment-2", ReleaseId = releases[1].Id, EnvironmentId = deploymentEnvironments[0].Id, DeployedAt = DateTime.Now.AddDays(-2) },
                new Deployment { Id = "Deployment-3", ReleaseId = releases[2].Id, EnvironmentId = deploymentEnvironments[0].Id, DeployedAt = DateTime.Now.AddDays(-1) }
            };

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(2);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == "Release-2");
            Assert.Contains(result, r => r.Id == "Release-3");
        }

        [Fact]
        public void ShouldReturnAllDeployedReleases_WhenFewerDeployedReleasesExist_ThanRetentionLimit()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);

            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", ReleaseId = releases[0].Id, EnvironmentId = deploymentEnvironments[0].Id, DeployedAt = DateTime.Now.AddDays(-5) },
                new Deployment { Id = "Deployment-2", ReleaseId = releases[1].Id, EnvironmentId = deploymentEnvironments[0].Id, DeployedAt = DateTime.Now.AddDays(-2) },
            };

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(5);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == "Release-1");
            Assert.Contains(result, r => r.Id == "Release-2");
        }

        [Fact]
        public void ShouldReturnCorrectReleases_WhenSameReleaseDeployedToMultipleEnvironments()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 1, deploymentEnvironmentsNum: 3);

            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", ReleaseId = releases[0].Id, EnvironmentId = deploymentEnvironments[0].Id, DeployedAt = DateTime.Now.AddDays(-5) },
                new Deployment { Id = "Deployment-2", ReleaseId = releases[0].Id, EnvironmentId = deploymentEnvironments[1].Id, DeployedAt = DateTime.Now.AddDays(-2) },
                new Deployment { Id = "Deployment-3", ReleaseId = releases[0].Id, EnvironmentId = deploymentEnvironments[2].Id, DeployedAt = DateTime.Now.AddDays(-1) }
            };

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(2);

            // Assert
            Assert.Single(result);
            Assert.Contains(result, r => r.Id == "Release-1");
        }

        [Fact]
        public void ShouldReturnNoReleases_WhenNoDeploymentsExist()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 1, deploymentEnvironmentsNum: 1);

            var deployments = new List<Deployment>();

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(2);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ShouldReturnCorrectReleases_WhenSomeReleasesHaveNoDeployments()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);

            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[2].Id, DeployedAt = DateTime.Now },
            };

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(2);

            // Assert
            Assert.Single(result);
            Assert.Contains(result, r => r.Id == "Release-3");
        }

        [Fact]
        public void ShouldReturnCorrectReleases_WhenDeploymentsToMultipleEnvironmentsExist()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 1, deploymentEnvironmentsNum: 2);

            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[0].Id, DeployedAt = DateTime.Now },
                new Deployment { Id = "Deployment-2", EnvironmentId = deploymentEnvironments[1].Id, ReleaseId = releases[0].Id, DeployedAt = DateTime.Now.AddDays(-1)},
            };

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(1);

            // Assert
            Assert.Single(result);
            Assert.Contains(result, r => r.Id == "Release-1");
        }

        [Fact]
        public void ShouldLogCorrectMessage_WhenReleaseIsKept()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 1, deploymentEnvironmentsNum: 1);

            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[0].Id, DeployedAt = new DateTime(2024, 01, 01) },
            };

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            _sut.GetReleasesToKeep(1);

            // Assert
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg =>
                msg == "Release Release-1 (Version: 1.0.1) was retained because it is within 1 most recent deployed in Environment-1. Deployment date is: 1/01/2024 12:00:00 AM")), Times.Once);
        }

        [Fact]
        public void ShouldLogCorrectNumberOfMessages_WhenMultipleReleasesAreKept()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);

            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[0].Id, DeployedAt = DateTime.Now.AddDays(-2) },
                new Deployment { Id = "Deployment-2", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[1].Id, DeployedAt = DateTime.Now.AddDays(-1) },
                new Deployment { Id = "Deployment-3", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[2].Id, DeployedAt = DateTime.Now },
            };

            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            _sut.GetReleasesToKeep(5);

            // Assert
            _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Exactly(3));
        }

        [Fact]
        public void ShouldLog_No_Message_WhenNoReleasesToKeep()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 1, deploymentEnvironmentsNum: 1);
            var deployments = new List<Deployment>();
            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            _sut.GetReleasesToKeep(1);

            // Assert
            _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ShouldReturnTaggedRelease_WhenNoDeployedReleasesExist()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);
            releases[2].Tag = "prod";
            var deployments = new List<Deployment>();
            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(1);

            // Assert
            Assert.Single(result);
            Assert.Contains(result, r => r.Id == "Release-3"); // tagged
        }

        [Fact]
        public void ShouldLogTaggedRelease_WhenNoDeployedReleasesExist()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);
            releases[2].Tag = "prod";
            var deployments = new List<Deployment>();
            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            _sut.GetReleasesToKeep(1);

            // Assert
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg == "Release Release-3 (Version: 1.0.3) was retained because it is tagged = 'prod'")), Times.Once);
        }

        [Fact]
        public void ShouldReturnTaggedAndDeployedReleases_WhenTaggedAndDeployedReleasesExist()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);
            releases[0].Tag = "prod";
            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[1].Id, DeployedAt = new DateTime(2024, 1, 1) },
                new Deployment { Id = "Deployment-2", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[2].Id, DeployedAt = new DateTime(2024, 1, 2) },
            };
            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            var result = _sut.GetReleasesToKeep(1);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.Id == "Release-1"); // tagged
            Assert.Contains(result, r => r.Id == "Release-3"); // 1 most resently deployed
        }

        [Fact]
        public void ShouldLogTaggedAndDeployedReleases_WhenTaggedAndDeployedReleasesExist()
        {
            // Arrange
            var (projects, releases, deploymentEnvironments) = GetDummyData(projectsNum: 1, releasesNum: 3, deploymentEnvironmentsNum: 1);
            releases[0].Tag = "prod";
            var deployments = new List<Deployment>
            {
                new Deployment { Id = "Deployment-1", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[1].Id, DeployedAt = new DateTime(2024, 1, 1) },
                new Deployment { Id = "Deployment-2", EnvironmentId = deploymentEnvironments[0].Id, ReleaseId = releases[2].Id, DeployedAt = new DateTime(2024, 1, 2) },
            };
            SetupMocks(projects, releases, deploymentEnvironments, deployments);

            // Act
            _sut.GetReleasesToKeep(1);

            // Assert
            _loggerMock.Verify(logger => logger.Log(It.IsAny<string>()), Times.Exactly(2));
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg == "Release Release-1 (Version: 1.0.1) was retained because it is tagged = 'prod'")), Times.Once);
            _loggerMock.Verify(logger => logger.Log(It.Is<string>(msg => msg == "Release Release-3 (Version: 1.0.3) was retained because it is within 1 most recent deployed in Environment-1. Deployment date is: 2/01/2024 12:00:00 AM")), Times.Once);
        }

    };

    private List<Project> GetDummyProjects(int num)
    {
        var projects = new List<Project>();
        for (int i = 1; i <= num; i++)
        {
            projects.Add(new Project { Id = $"Project-{i}", Name = $"Project-{i} Name" });
        }
        return projects;
    }
    private List<DeploymentEnvironment> GetDummyDeploymentEnvironments(int num)
    {
        var deploymentEnvironments = new List<DeploymentEnvironment>();
        for (int i = 1; i <= num; i++)
        {
            deploymentEnvironments.Add(new DeploymentEnvironment { Id = $"Environment-{i}", Name = $"Environment-{i} Name" });
        }
        return deploymentEnvironments;
    }
    private List<Release> GetDummyReleases(int num, List<Project> projects)
    {
        // Spread releases across all available projects. E.g. when creating 10 releases and having 2 projects
        // the releases will be evenly distributed across the projects.
        var currProject = 0;
        var releasesPerProject = num / projects.Count;
        var releases = new List<Release>();
        for (int i = 1; i <= num; i++)
        {
            if (i > releasesPerProject * (currProject + 1))
            {
                currProject++;
            }

            releases.Add(new Release { Id = $"Release-{i}", ProjectId = projects[currProject].Id, Version = $"1.0.{i}", Created = DateTime.Now.AddDays(num - i) });
        }

        return releases;
    }
    private ValueTuple<List<Project>, List<Release>, List<DeploymentEnvironment>> GetDummyData(int projectsNum, int releasesNum, int deploymentEnvironmentsNum)
    {
        var projects = GetDummyProjects(projectsNum);
        var releases = GetDummyReleases(releasesNum, projects);
        var deploymentEnvironments = GetDummyDeploymentEnvironments(deploymentEnvironmentsNum);

        return (projects, releases, deploymentEnvironments);
    }
    private void SetupMocks(List<Project> projects, List<Release> releases, List<DeploymentEnvironment> deploymentEnvironments, List<Deployment> deployments)
    {
        _projectRepositoryMock.Setup(repo => repo
            .GetAll(It.IsAny<Func<Project, bool>?>()))
            .Returns<Func<Project, bool>?>(f => f == null ? projects : projects.Where(f).ToList());

        _releaseRepositoryMock.Setup(repo => repo
            .GetAll(It.IsAny<Func<Release, bool>?>()))
            .Returns<Func<Release, bool>?>(f => f == null ? releases : releases.Where(f).ToList());

        _releaseRepositoryMock.Setup(repo => repo.GetById(It.IsAny<string>())).Returns<string>(id => releases.FirstOrDefault(r => r.Id == id));

        _deploymentEnvironmentRepositoryMock.Setup(repo => repo
            .GetAll(It.IsAny<Func<DeploymentEnvironment, bool>?>()))
            .Returns<Func<DeploymentEnvironment, bool>?>(filter => filter == null ? deploymentEnvironments : deploymentEnvironments.Where(filter).ToList());

        _deploymentRepositoryMock.Setup(repo => repo
            .GetAll(It.IsAny<Func<Deployment, bool>?>()))
            .Returns<Func<Deployment, bool>?>(filter => filter == null ? deployments : deployments.Where(filter).ToList());
    }
}

