using System.Collections.Generic;
using System.Threading.Tasks;
using TechnicalDocsAssistant.Core.Models;

namespace TechnicalDocsAssistant.Core.Interfaces
{
    public interface IAssetService
    {
        Task<Asset> GetAssetByIdAsync(string id);
        Task<IEnumerable<Asset>> GetAllAssetsAsync();
        Task<Asset> CreateAssetAsync(string title, string markdownContent);
        Task<Asset> UpdateAssetAsync(string id, string title, string markdownContent);
        Task DeleteAssetAsync(string id);
        Task UpdateEmbeddingsAsync();
        Task<IEnumerable<Asset>> GetSimilarAssetsAsync(float[] queryVector, int limit = 5, float similarityThreshold = 0.3f);
    }
}
