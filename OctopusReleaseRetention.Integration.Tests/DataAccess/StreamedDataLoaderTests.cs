using OctopusReleaseRetention.Integration.Tests;
using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.DataAccess.Integration.Tests
{
    public class StreamedDataLoaderTests
    {
        private readonly IDataLoader _sut;
        private readonly ILogger _logger;

        public StreamedDataLoaderTests()
        {
            _logger = new InMemoryLogger();
            _sut = new StreamedDataLoader("TestData", _logger);
        }

        public class LoadData : StreamedDataLoaderTests
        {
            [Fact]
            public void LoadData_ShouldLoadCorrectProjectData()
            {
                // Arrange
                var projects = "Projects.json";

                // Act
                var result = _sut.LoadData<Project>(projects).ToList();

                // Assert
                Assert.Equal(2, result.Count);
                Assert.Contains(result, p => p.Id == "Project-1");
                Assert.Contains(result, p => p.Id == "Project-2");
            }
        }
    }
}