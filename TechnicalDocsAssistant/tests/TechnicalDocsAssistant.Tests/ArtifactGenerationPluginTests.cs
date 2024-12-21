using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.SKPlugins;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace TechnicalDocsAssistant.Tests
{
    public class ArtifactGenerationPluginTests
    {
        private readonly Kernel _kernel;
        private readonly ArtifactGenerationPlugin _plugin;
        private readonly UserStory _sampleUserStory;

        public ArtifactGenerationPluginTests()
        {
            // Load configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var openAIConfig = configuration.GetSection("OpenAI");
            var modelId = openAIConfig["DeploymentName"] 
                ?? throw new InvalidOperationException("OpenAI:DeploymentName is not configured");
            var apiKey = openAIConfig["ApiKey"]
                ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");

            // Initialize Semantic Kernel
            var builder = Kernel.CreateBuilder()
                .AddOpenAIChatCompletion(
                    modelId: modelId,
                    apiKey: apiKey
                );
            _kernel = builder.Build();
            _plugin = new ArtifactGenerationPlugin(_kernel);

            // Create a sample user story for testing
            _sampleUserStory = new UserStory
            {
                Title = "User Login Feature",
                Description = "As a registered user, I want to log in to the system so that I can access my account",
                Actors = new List<string> { "Registered User", "Authentication System" },
                Preconditions = new List<string> { "User has a registered account", "User is not logged in" },
                MainFlow = new List<string>
                {
                    "1. User navigates to the login page",
                    "2. User enters their username and password",
                    "3. System validates the credentials",
                    "4. System grants access to the user's account"
                },
                AlternativeFlows = new List<string>
                {
                    "3a. If credentials are invalid, show error message and return to step 2",
                    "3b. If account is locked, show account locked message and provide recovery options"
                },
                BusinessRules = new List<string>
                {
                    "Account should be locked after 3 failed attempts",
                    "Password must meet security requirements"
                }
            };
        }

        [Fact]
        public async Task GenerateFlowchart_ShouldReturnValidPlantUMLCode()
        {
            // Arrange
            var userStoryJson = JsonSerializer.Serialize(_sampleUserStory);

            // Act
            var result = await _plugin.GenerateFlowchart(userStoryJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("@startuml", result);
            Assert.Contains("@enduml", result);
            Assert.Contains("login", result.ToLower());
            Assert.DoesNotContain("error generating", result.ToLower());
        }

        [Fact]
        public async Task GenerateSequenceDiagram_ShouldReturnValidPlantUMLCode()
        {
            // Arrange
            var userStoryJson = JsonSerializer.Serialize(_sampleUserStory);

            // Act
            var result = await _plugin.GenerateSequenceDiagram(userStoryJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("@startuml", result);
            Assert.Contains("@enduml", result);
            Assert.Contains("User", result);
            Assert.Contains("->", result);
            Assert.DoesNotContain("error generating", result.ToLower());
        }

        [Fact]
        public async Task GenerateTestCases_ShouldReturnValidMarkdown()
        {
            // Arrange
            var userStoryJson = JsonSerializer.Serialize(_sampleUserStory);

            // Act
            var result = await _plugin.GenerateTestCases(userStoryJson);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("### Test Case", result);
            Assert.Contains("**Description**", result);
            Assert.Contains("**Steps**", result);
            Assert.Contains("**Expected Result**", result);
            Assert.DoesNotContain("error generating", result.ToLower());
        }

        [Fact]
        public async Task GenerateFlowchart_WithNullInput_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _plugin.GenerateFlowchart(string.Empty));
        }

        [Fact]
        public async Task GenerateSequenceDiagram_WithNullInput_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _plugin.GenerateSequenceDiagram(string.Empty));
        }

        [Fact]
        public async Task GenerateTestCases_WithNullInput_ShouldThrowArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _plugin.GenerateTestCases(string.Empty));
        }
    }
}
