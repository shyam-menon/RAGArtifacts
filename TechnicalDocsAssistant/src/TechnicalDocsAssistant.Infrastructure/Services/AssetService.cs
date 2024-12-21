using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Core.Models;
using System.Text.Json;
using Supabase;
using Postgrest;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Extensions.DependencyInjection;

#pragma warning disable SKEXP0001 // Disable warning about experimental features

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public class AssetService : IAssetService
    {
        private readonly Kernel _kernel;
        private readonly Supabase.Client _supabaseClient;

        public AssetService(
            Kernel kernel,
            string supabaseUrl,
            string supabaseKey)
        {
            _kernel = kernel;
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            _supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
        }

        public async Task<Asset> GetAssetByIdAsync(string id)
        {
            var result = await _supabaseClient
                .From<Asset>()
                .Filter("id", Postgrest.Constants.Operator.Equals, id)
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "false")
                .Get();

            return result.Models.FirstOrDefault();
        }

        public async Task<IEnumerable<Asset>> GetAllAssetsAsync()
        {
            var result = await _supabaseClient
                .From<Asset>()
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "false")
                .Get();

            return result.Models;
        }

        public async Task<Asset> CreateAssetAsync(string title, string markdownContent)
        {
              // Get the embedding generation service
            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            
            // Generate embeddings
            var embedding = await embeddingGenerator.GenerateEmbeddingAsync(markdownContent);
            
            var asset = new Asset
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                MarkdownContent = markdownContent,
                ContentVector = embedding.ToArray(),
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                IsDeleted = false
            };

            var dbResult = await _supabaseClient
                .From<Asset>()
                .Insert(asset);

            return dbResult.Models[0];
        }

       public async Task<Asset> UpdateAssetAsync(string id, string title, string markdownContent)
        {
            var existing = await GetAssetByIdAsync(id);
            if (existing == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found");

            // Get the embedding generation service and generate new embeddings
            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            var embedding = await embeddingGenerator.GenerateEmbeddingAsync(markdownContent);

            existing.Title = title;
            existing.MarkdownContent = markdownContent;
            existing.ContentVector = embedding.ToArray();
            existing.Modified = DateTime.UtcNow;

            var dbResult = await _supabaseClient
                .From<Asset>()
                .Update(existing);

            return dbResult.Models[0];
        }

        public async Task DeleteAssetAsync(string id)
        {
            var asset = await GetAssetByIdAsync(id);
            if (asset == null)
                throw new KeyNotFoundException($"Asset with ID {id} not found");

            asset.IsDeleted = true;
            await _supabaseClient
                .From<Asset>()
                .Update(asset);
        }

        public async Task<IEnumerable<Asset>> SearchSimilarAssetsAsync(string query, int limit = 5)
        {
            // Get the embedding generation service and generate query embeddings
            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            var queryEmbedding = await embeddingGenerator.GenerateEmbeddingAsync(query);
            var queryVector = queryEmbedding.ToArray();

            // Search using vector similarity
            var dbResult = await _supabaseClient.Rpc(
                "search_assets_by_similarity",
                new Dictionary<string, object>
                {
                    { "query_embedding", queryVector },
                    { "match_threshold", 0.7 },
                    { "match_count", limit }
                });

            var assets = JsonSerializer.Deserialize<List<Asset>>(dbResult.ToString());
            return assets ?? new List<Asset>();
        }
    }
}
