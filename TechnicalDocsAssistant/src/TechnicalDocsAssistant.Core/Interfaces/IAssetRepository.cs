using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TechnicalDocsAssistant.Core.Models;

namespace TechnicalDocsAssistant.Core.Interfaces
{
    public interface IAssetRepository
    {
        Task<Asset> GetByIdAsync(string id);
        Task<IEnumerable<Asset>> GetAllAsync();
        Task<Asset> CreateAsync(Asset asset);
        Task<Asset> UpdateAsync(Asset asset);
        Task DeleteAsync(string id);
        Task<IEnumerable<Asset>> SearchSimilarAsync(byte[] vectorQuery, int limit = 5);
    }
}
