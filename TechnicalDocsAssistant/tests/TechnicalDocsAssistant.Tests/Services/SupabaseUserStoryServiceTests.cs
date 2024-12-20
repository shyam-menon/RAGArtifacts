using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Infrastructure.Services;
using FluentAssertions;

namespace TechnicalDocsAssistant.Tests.Services
{
    public class SupabaseUserStoryServiceTests : IAsyncLifetime
    {
        private readonly SupabaseUserStoryService _service;
        private readonly string _testId;

        public SupabaseUserStoryServiceTests()
        {
            _service = new SupabaseUserStoryService(TestHelper.GetSupabaseUrl(), TestHelper.GetSupabaseKey());
            _testId = Guid.NewGuid().ToString();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            try
            {
                await _service.DeleteUserStoryAsync(_testId);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        [Fact]
        public async Task CreateUserStoryAsync_ShouldCreateAndReturnUserStory()
        {
            // Arrange
            var userStory = new UserStory
            {
                Id = _testId,
                Title = "Test User Story",
                Description = "As a user, I want to test the service",
                Actors = new List<string> { "User" },
                Preconditions = new List<string> { "Service is running" },
                Postconditions = new List<string> { "Test is completed" },
                MainFlow = new List<string> { "User creates a story", "System saves the story" },
                AlternativeFlows = new List<string> { "System fails to save" },
                BusinessRules = new List<string> { "Story must have a title" },
                DataRequirements = new List<string> { "Story data" },
                NonFunctionalRequirements = new List<string> { "Response within 1s" },
                Assumptions = new List<string> { "Database is available" }
            };

            // Act
            var result = await _service.CreateUserStoryAsync(userStory);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(_testId);
            result.Title.Should().Be(userStory.Title);
            result.Description.Should().Be(userStory.Description);
            result.CreatedAt.Should().NotBe(default);
        }

        [Fact]
        public async Task GetUserStoryByIdAsync_ShouldReturnUserStory()
        {
            // Arrange
            var userStory = new UserStory
            {
                Id = _testId,
                Title = "Test User Story",
                Description = "Test Description"
            };
            await _service.CreateUserStoryAsync(userStory);

            // Act
            var result = await _service.GetUserStoryByIdAsync(_testId);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(_testId);
            result.Title.Should().Be(userStory.Title);
            result.Description.Should().Be(userStory.Description);
        }

        [Fact]
        public async Task UpdateUserStoryAsync_ShouldUpdateAndReturnUserStory()
        {
            // Arrange
            var userStory = new UserStory
            {
                Id = _testId,
                Title = "Original Title",
                Description = "Original Description"
            };
            await _service.CreateUserStoryAsync(userStory);

            // Update the story
            userStory.Title = "Updated Title";
            userStory.Description = "Updated Description";

            // Act
            var result = await _service.UpdateUserStoryAsync(userStory);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(_testId);
            result.Title.Should().Be("Updated Title");
            result.Description.Should().Be("Updated Description");
            result.UpdatedAt.Should().BeAfter(result.CreatedAt);
        }

        [Fact]
        public async Task GetAllUserStoriesAsync_ShouldReturnAllUserStories()
        {
            // Arrange
            var userStory = new UserStory
            {
                Id = _testId,
                Title = "Test User Story",
                Description = "Test Description"
            };
            await _service.CreateUserStoryAsync(userStory);

            // Act
            var result = await _service.GetAllUserStoriesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().Contain(x => x.Id == _testId);
        }

        [Fact]
        public async Task DeleteUserStoryAsync_ShouldDeleteUserStory()
        {
            // Arrange
            var userStory = new UserStory
            {
                Id = _testId,
                Title = "Test User Story",
                Description = "Test Description"
            };
            await _service.CreateUserStoryAsync(userStory);

            // Act
            await _service.DeleteUserStoryAsync(_testId);
            var result = await _service.GetUserStoryByIdAsync(_testId);

            // Assert
            result.Should().BeNull();
        }
    }
}
