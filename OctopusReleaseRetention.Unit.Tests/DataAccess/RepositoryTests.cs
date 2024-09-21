using OctopusReleaseRetention.Entities;
using OctopusReleaseRetention.Interfaces;

namespace OctopusReleaseRetention.DataAccess.Unit.Tests
{
    public class RepositoryTests
    {
        private readonly IRepository<Project> _sut;

        public RepositoryTests()
        {
            var dummyProjects = new List<Project>
            {
                new Project { Id = "Project-1", Name = "Project-1 Name" },
                new Project { Id = "Project-2", Name = "Project-2 Name" }
            };

            _sut = new Repository<Project>(dummyProjects);
        }

        [Fact]
        public void GetAll_ShouldReturnAllItems()
        {
            // Act
            var result = _sut.GetAll().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Id == "Project-1");
            Assert.Contains(result, p => p.Id == "Project-2");
        }

        [Fact]
        public void GetById_ShouldReturnCorrectItem()
        {
            // Act
            var result = _sut.GetById("Project-2");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Project-2", result.Id);
        }

        [Fact]
        public void Add_ShouldAddItemToRepository()
        {
            // Arrange
            var newProject = new Project { Id = "Project-3", Name = "Project-3 Name" };

            // Act
            _sut.Add(newProject);

            // Assert
            var result = _sut.GetAll().ToList();
            Assert.Equal(3, result.Count); // Initially 2, added 1 more
            Assert.Contains(result, p => p.Id == "Project-3");
        }

        [Fact]
        public void AddRange_ShouldAddMultipleItemsToRepository()
        {
            // Arrange
            var newProjects = new List<Project>
            {
                new Project { Id = "Project-4", Name = "Project-4 Name" },
                new Project { Id = "Project-5", Name = "Project-5 Name" }
            };

            // Act
            _sut.AddRange(newProjects);

            // Assert
            var result = _sut.GetAll();
            Assert.Equal(4, result.Count()); // Initially 2, added 2 more
            Assert.Contains(result, p => p.Id == "Project-4");
            Assert.Contains(result, p => p.Id == "Project-5");
        }

        [Fact]
        public void AddRange_ShouldHandleEmptyCollection()
        {
            // Arrange
            var emptyList = new List<Project>();

            // Act
            _sut.AddRange(emptyList);

            // Assert
            var result = _sut.GetAll();
            Assert.Equal(2, result.Count());
        }
    }
}