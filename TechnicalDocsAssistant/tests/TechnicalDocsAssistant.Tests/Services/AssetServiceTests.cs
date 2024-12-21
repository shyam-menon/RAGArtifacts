using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Moq;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Infrastructure.Services;
using Xunit;

namespace TechnicalDocsAssistant.Tests.Services
{
    public class AssetServiceTests
    {
        private readonly Mock<ITextEmbeddingGenerationService> _mockEmbeddingService;
        private readonly Mock<Kernel> _mockKernel;
        private readonly string _supabaseUrl = "http://localhost:54321";
        private readonly string _supabaseKey = "test-key";

        public AssetServiceTests()
        {
            _mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>();
            _mockKernel = new Mock<Kernel>();
            
            // Setup mock kernel to return the embedding service
            _mockKernel.Setup(k => k.Services)
                .Returns(new ServiceCollection()
                    .AddSingleton(_mockEmbeddingService.Object)
                    .BuildServiceProvider());
        }

        [Fact]
        public async Task CreateAssetAsync_ShouldGenerateEmbeddingsAndCreateAsset()
        {
            // Arrange
            var title = "Test Asset";
            var content = "Test content for embedding generation";
            var expectedEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
            string? actualContent = null;

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), null))
                .Callback<string, Kernel?>((s, _) => actualContent = s)
                .ReturnsAsync(expectedEmbedding);

            var service = new AssetService(_mockKernel.Object, _supabaseUrl, _supabaseKey);

            // Act
            var result = await service.CreateAssetAsync(title, content);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.Equal(content, result.MarkdownContent);
            Assert.Equal(expectedEmbedding.ToArray(), result.ContentVector);
            Assert.False(result.IsDeleted);

            // Verify embedding service was called with correct content
            Assert.Equal(content, actualContent);
            _mockEmbeddingService.Verify(
                s => s.GenerateEmbeddingAsync(It.IsAny<string>(), null),
                Times.Once);
        }

        [Fact]
        public async Task SearchSimilarAssetsAsync_ShouldGenerateQueryEmbeddingsAndSearch()
        {
            // Arrange
            var query = "test query";
            var expectedEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });
            string? actualQuery = null;

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), null))
                .Callback<string, Kernel?>((s, _) => actualQuery = s)
                .ReturnsAsync(expectedEmbedding);

            var service = new AssetService(_mockKernel.Object, _supabaseUrl, _supabaseKey);

            // Act
            var results = await service.SearchSimilarAssetsAsync(query);

            // Assert
            Assert.NotNull(results);

            // Verify embedding service was called with correct query
            Assert.Equal(query, actualQuery);
            _mockEmbeddingService.Verify(
                s => s.GenerateEmbeddingAsync(It.IsAny<string>(), null),
                Times.Once);
        }

        [Fact]
        public async Task UpdateAssetAsync_ShouldGenerateNewEmbeddingsAndUpdate()
        {
            // Arrange
            var id = "test-id";
            var title = "Updated Asset";
            var content = "Updated content for embedding generation";
            var expectedEmbedding = new ReadOnlyMemory<float>(new float[] { 0.4f, 0.5f, 0.6f });
            string? actualContent = null;

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), null))
                .Callback<string, Kernel?>((s, _) => actualContent = s)
                .ReturnsAsync(expectedEmbedding);

            var service = new AssetService(_mockKernel.Object, _supabaseUrl, _supabaseKey);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                service.UpdateAssetAsync(id, title, content));

            // Verify embedding service was not called since asset was not found
            Assert.Null(actualContent);
            _mockEmbeddingService.Verify(
                s => s.GenerateEmbeddingAsync(It.IsAny<string>(), null),
                Times.Never);
        }
    }
}
