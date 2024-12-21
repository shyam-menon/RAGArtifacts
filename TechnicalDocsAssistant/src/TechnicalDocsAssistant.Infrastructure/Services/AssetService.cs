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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

#pragma warning disable SKEXP0001 // Disable warning about experimental features

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public class AssetService : IAssetService
    {
        private readonly Supabase.Client _supabaseClient;
        private readonly Kernel _kernel;
        private readonly JsonSerializerSettings _jsonSettings;

        public AssetService(Supabase.Client supabaseClient, Kernel kernel)
        {
            _supabaseClient = supabaseClient;
            _kernel = kernel;
            _jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Converters = new List<JsonConverter> { new VectorJsonConverter() }
            };
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
            Console.WriteLine("Getting all assets from database");
            var result = await _supabaseClient
                .From<Asset>()
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "false")
                .Get();

            var assets = result.Models;
            foreach (var asset in assets)
            {
                Console.WriteLine($"Retrieved asset {asset.Id} - {asset.Title}");
                if (asset.ContentVector != null)
                {
                    Console.WriteLine($"Asset has embedding with dimension: {asset.ContentVector.Length}");
                }
                else
                {
                    Console.WriteLine("Asset has no embedding");
                }
            }

            return assets;
        }

        public async Task<Asset> CreateAssetAsync(string title, string markdownContent)
        {
            // Get the embedding generation service
            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            
            // Generate embeddings
            Console.WriteLine($"Generating embeddings for asset: {title}");
            var embedding = await embeddingGenerator.GenerateEmbeddingAsync(markdownContent);
            var embeddingArray = embedding.ToArray();
            Console.WriteLine($"Generated embedding with dimension: {embeddingArray.Length}");
            Console.WriteLine($"First few values: {string.Join(", ", embeddingArray.Take(5))}");
            
            var asset = new Asset
            {
                Id = Guid.NewGuid().ToString(),
                Title = title,
                MarkdownContent = markdownContent,
                ContentVector = embeddingArray,
                Created = DateTime.UtcNow,
                Modified = DateTime.UtcNow,
                IsDeleted = false
            };

            Console.WriteLine($"Saving asset to database with embedding dimension: {asset.ContentVector.Length}");
            Console.WriteLine($"First few values before save: {string.Join(", ", asset.ContentVector.Take(5))}");

            // Convert to JSON to see what's being sent
            var json = JsonConvert.SerializeObject(asset, _jsonSettings);
            Console.WriteLine($"Asset JSON: {json}");

            var dbResult = await _supabaseClient
                .From<Asset>()
                .Insert(asset);

            var savedAsset = dbResult.Models.FirstOrDefault();
            if (savedAsset == null)
            {
                throw new InvalidOperationException("Failed to save asset");
            }

            if (savedAsset.ContentVector != null)
            {
                Console.WriteLine($"Asset saved with embedding dimension: {savedAsset.ContentVector.Length}");
                Console.WriteLine($"First few values after save: {string.Join(", ", savedAsset.ContentVector.Take(5))}");
            }
            else
            {
                Console.WriteLine("Warning: Saved asset has no embedding");
            }

            return savedAsset;
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

            return dbResult.Models.FirstOrDefault();
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

            var assets = JsonConvert.DeserializeObject<List<Asset>>(dbResult.ToString(), _jsonSettings);
            return assets ?? new List<Asset>();
        }

        public async Task<IEnumerable<Asset>> GetSimilarAssetsAsync(float[] queryVector, int limit = 5, float similarityThreshold = 0.3f)
        {
            Console.WriteLine($"Getting similar assets with threshold {similarityThreshold} and limit {limit}");
            Console.WriteLine($"Query vector dimension: {queryVector.Length}");
            Console.WriteLine($"First few values: {string.Join(", ", queryVector.Take(5))}");

            // Convert the vector to a string array for Supabase
            var queryVectorStr = $"[{string.Join(",", queryVector)}]";
            Console.WriteLine($"Query vector string: {queryVectorStr.Substring(0, Math.Min(100, queryVectorStr.Length))}...");

            try
            {
                Console.WriteLine("Calling Supabase RPC...");
                var dbResult = await _supabaseClient.Rpc(
                    "search_assets_by_similarity",
                    new Dictionary<string, object>
                    {
                        { "match_count", limit },
                        { "match_threshold", similarityThreshold },
                        { "query_embedding", queryVectorStr }
                    });

                Console.WriteLine($"Raw response from Supabase: {dbResult}");
                Console.WriteLine($"Response type: {dbResult?.GetType().FullName}");
                Console.WriteLine($"Response content: {dbResult?.ToString()}");

                if (dbResult == null)
                {
                    Console.WriteLine("Supabase returned null response");
                    return new List<Asset>();
                }

                try
                {
                    var responseStr = dbResult.ToString();
                    Console.WriteLine($"Response string length: {responseStr.Length}");
                    Console.WriteLine($"First 100 chars of response: {responseStr.Substring(0, Math.Min(100, responseStr.Length))}");

                    // Try to get the response as a Postgrest response
                    if (dbResult is Postgrest.Responses.BaseResponse baseResponse)
                    {
                        Console.WriteLine("Got Postgrest response");
                        responseStr = baseResponse.Content;
                        Console.WriteLine($"Content: {responseStr}");
                    }

                    // If the response starts with '[' and ends with ']', it's already a JSON array
                    // Otherwise, wrap it in square brackets to make it a JSON array
                    if (!responseStr.StartsWith("[") && !responseStr.EndsWith("]"))
                    {
                        responseStr = $"[{responseStr}]";
                    }

                    var assets = JsonConvert.DeserializeObject<List<Asset>>(responseStr, _jsonSettings);
                    if (assets != null)
                    {
                        Console.WriteLine($"Found {assets.Count} similar assets");
                        foreach (var asset in assets)
                        {
                            Console.WriteLine($"Asset {asset.Id} - {asset.Title}");
                            if (asset.ContentVector != null)
                            {
                                Console.WriteLine($"Asset has embedding with dimension: {asset.ContentVector.Length}");
                                Console.WriteLine($"First few values: {string.Join(", ", asset.ContentVector.Take(5))}");
                            }
                            else
                            {
                                Console.WriteLine("Asset has no embedding");
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("No similar assets found");
                    }

                    return assets ?? new List<Asset>();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing response: {ex}");
                    Console.WriteLine($"Error type: {ex.GetType().FullName}");
                    Console.WriteLine($"Error message: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Supabase RPC: {ex}");
                Console.WriteLine($"Error type: {ex.GetType().FullName}");
                Console.WriteLine($"Error message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task UpdateEmbeddingsAsync()
        {
            var assets = await GetAllAssetsAsync();
            var embeddingGenerator = _kernel.GetRequiredService<ITextEmbeddingGenerationService>();

            foreach (var asset in assets)
            {
                if (!string.IsNullOrEmpty(asset.MarkdownContent))
                {
                    Console.WriteLine($"Updating embeddings for asset {asset.Id} - {asset.Title}");
                    var embedding = await embeddingGenerator.GenerateEmbeddingAsync(asset.MarkdownContent);
                    asset.ContentVector = embedding.ToArray();
                    asset.Modified = DateTime.UtcNow;

                    await _supabaseClient
                        .From<Asset>()
                        .Update(asset);

                    Console.WriteLine($"Updated embeddings for asset {asset.Id}");
                }
            }
        }
    }
}
