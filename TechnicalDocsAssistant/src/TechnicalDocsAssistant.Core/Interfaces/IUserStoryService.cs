using System.Collections.Generic;
using System.Threading.Tasks;
using TechnicalDocsAssistant.Core.Models;

namespace TechnicalDocsAssistant.Core.Interfaces
{
    public interface IUserStoryService
    {
        Task<UserStory> CreateUserStoryAsync(UserStory userStory);
        Task<UserStory> UpdateUserStoryAsync(UserStory userStory);
        Task<UserStory?> GetUserStoryByIdAsync(string id);
        Task<List<UserStory>> GetAllUserStoriesAsync();
        Task DeleteUserStoryAsync(string id);
    }
}
