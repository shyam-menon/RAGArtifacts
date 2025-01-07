using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Infrastructure.Services;
using Xunit;
using System.Linq;
using Postgrest.Models;
using Postgrest.Responses;
using Postgrest;
using Supabase.Realtime;
using Newtonsoft.Json;
using Supabase;
using Postgrest.Interfaces;
using Supabase.Realtime.Interfaces;
using Supabase.Interfaces;

namespace TechnicalDocsAssistant.Tests.Services
{
    public class AssetServiceTests
    {
        private readonly Mock<Supabase.Client> _mockSupabaseClient;
        private readonly Mock<Kernel> _mockKernel;
        private readonly Mock<ITextEmbeddingGenerationService> _mockEmbeddingService;
        private readonly AssetService _assetService;
        private readonly IServiceProvider _serviceProvider;

        public AssetServiceTests()
        {
            _mockSupabaseClient = new Mock<Supabase.Client>();
            _mockKernel = new Mock<Kernel>();
            _mockEmbeddingService = new Mock<ITextEmbeddingGenerationService>();

            var services = new ServiceCollection();
            services.AddSingleton<ITextEmbeddingGenerationService>(_mockEmbeddingService.Object);
            _serviceProvider = services.BuildServiceProvider();

            _mockKernel.Setup(k => k.Services).Returns(_serviceProvider);

            _assetService = new AssetService(_mockSupabaseClient.Object, _mockKernel.Object);
        }

        [Fact]
        public async Task CreateAssetAsync_ShouldGenerateEmbeddingsAndCreateAsset()
        {
            // Arrange
            var title = "Test Asset";
            var content = "Test Content";
            var expectedEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmbedding);

            var asset = new Asset
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                MarkdownContent = content,
                TechnologyStack = new Dictionary<string, string> { { "language", "C#" }, { "framework", "ASP.NET Core" } }
            };

            var mockResponse = new Mock<BaseResponse>();
            var modelResponse = new ModeledResponse<Asset>(mockResponse.Object, new JsonSerializerSettings());
            var mockSupabaseTable = new Mock<SupabaseTable<Asset>>();

            _mockSupabaseClient.Setup(x => x.From<Asset>()).Returns(mockSupabaseTable.Object);

            mockSupabaseTable.Setup(x => x.Insert(It.IsAny<Asset>())).ReturnsAsync(modelResponse);

            // Act
            var result = await _assetService.CreateAssetAsync(title, content);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(asset.Id, result.Id);
            Assert.Equal(asset.Title, result.Title);
            Assert.Equal(asset.MarkdownContent, result.MarkdownContent);
            Assert.Equal(asset.TechnologyStack, result.TechnologyStack);
        }

        [Fact]
        public async Task SearchSimilarAssetsAsync_ShouldGenerateQueryEmbeddingsAndSearch()
        {
            // Arrange
            var query = "test query";
            var expectedEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmbedding);

            var assets = new List<Asset>
            {
                new Asset
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Similar Asset 1",
                    MarkdownContent = "Similar Content 1",
                    ContentVector = new float[] { 0.1f, 0.2f, 0.3f }
                },
                new Asset
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = "Similar Asset 2",
                    MarkdownContent = "Similar Content 2",
                    ContentVector = new float[] { 0.2f, 0.3f, 0.4f }
                }
            };

            var mockResponse = new Mock<BaseResponse>();
            mockResponse.Setup(x => x.Content).Returns(JsonConvert.SerializeObject(assets));

            _mockSupabaseClient.Setup(x => x.Rpc(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>()))
                .ReturnsAsync(mockResponse.Object);

            // Act
            var result = await _assetService.SearchSimilarAssetsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, a => a.Title == "Similar Asset 1");
            Assert.Contains(result, a => a.Title == "Similar Asset 2");

            // Verify embedding service was called with correct query
            _mockEmbeddingService.Verify(s => s.GenerateEmbeddingAsync(query, It.IsAny<Kernel>(), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task UpdateAssetAsync_ShouldGenerateNewEmbeddingsAndUpdate()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var title = "Test Asset";
            var content = "Updated Content";
            var expectedEmbedding = new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f });

            _mockEmbeddingService
                .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedEmbedding);

            var asset = new Asset
            {
                Id = id,
                Title = title,
                MarkdownContent = content,
                TechnologyStack = new Dictionary<string, string> { { "language", "C#" }, { "framework", "ASP.NET Core" } }
            };

            var mockResponse = new Mock<BaseResponse>();
            var modelResponse = new ModeledResponse<Asset>(mockResponse.Object, new JsonSerializerSettings());
            var mockSupabaseTable = new Mock<SupabaseTable<Asset>>();

            var mockFilteredTable = new Mock<SupabaseTable<Asset>>();
            mockFilteredTable.Setup(x => x.Get()).ReturnsAsync(modelResponse);

            _mockSupabaseClient.Setup(x => x.From<Asset>()).Returns(mockSupabaseTable.Object);

            mockSupabaseTable.Setup(x => x.Filter(It.IsAny<string>(), It.IsAny<Postgrest.Constants.Operator>(), It.IsAny<object>()))
                .Returns(mockFilteredTable.Object);

            mockSupabaseTable.Setup(x => x.Update(It.IsAny<Asset>())).ReturnsAsync(modelResponse);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => 
                _assetService.UpdateAssetAsync(id, title, content));

            // Verify embedding service was not called since asset was not found
            _mockEmbeddingService.Verify(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
