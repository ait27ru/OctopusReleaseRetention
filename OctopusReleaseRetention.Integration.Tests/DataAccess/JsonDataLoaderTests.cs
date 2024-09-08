using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Integration.Tests;

namespace OctopusReleaseRetention.DataAccess.Integration.Tests
{
    public class JsonDataLoaderTests
    {
        private readonly InMemoryLogger _logger;
        private readonly JsonDataLoader _sut;

        public JsonDataLoaderTests()
        {
            _logger = new InMemoryLogger();
            _sut = new JsonDataLoader("TestData", _logger);
        }

        [Fact]
        public void LoadData_ShouldLoadCorrectProjectData()
        {
            // Arrange
            var projects = "Projects.json";

            // Act
            var result = _sut.LoadData<Project>(projects);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Id == "Project-1");
            Assert.Contains(result, p => p.Id == "Project-2");
        }

        [Fact]
        public void LoadData_ShouldLogAndThrow_WhenLoadIncorrectProjectData()
        {
            // Arrange
            var incorrectProjects = "Releases.json";

            // Act
            Assert.Throws<System.Text.Json.JsonException>(() => _sut.LoadData<Project>(incorrectProjects));

            // Assert
            var logs = _logger.GetLogs();
            Assert.Single(logs);
            Assert.Contains(logs, l => l == "Error deserialising from TestData\\Releases.json.");
        }

        [Fact]
        public void LoadData_ShouldLogAndThrow_WhenFileNotFound()
        {
            // Arrange
            var nonExistingProjects = "---";

            // Act, Assert
            Assert.Throws<FileNotFoundException>(() => _sut.LoadData<Project>(nonExistingProjects));
            var logs = _logger.GetLogs();
            Assert.Single(logs);
            Assert.Contains(logs, l => l == "File TestData\\--- not found.");
        }
    }
}