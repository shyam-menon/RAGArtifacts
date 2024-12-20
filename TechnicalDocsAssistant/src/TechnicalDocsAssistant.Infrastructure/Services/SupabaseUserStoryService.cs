using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Postgrest;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Core.Interfaces;
using Supabase.Gotrue;
using Postgrest.Models;

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public class SupabaseUserStoryService : IUserStoryService
    {
        private readonly Supabase.Client _supabaseClient;

        public SupabaseUserStoryService(string supabaseUrl, string supabaseKey)
        {
            var options = new Supabase.SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };
            _supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, options);
        }

        public async Task<UserStory> CreateUserStoryAsync(UserStory userStory)
        {
            var result = await _supabaseClient.From<UserStory>()
                .Insert(userStory);
            return result.Models[0];
        }

        public async Task<UserStory> UpdateUserStoryAsync(UserStory userStory)
        {
            userStory.UpdatedAt = DateTime.UtcNow;
            var query = _supabaseClient.From<UserStory>()
                .Where(x => x.Id == userStory.Id);
            var result = await query.Update(userStory);
            return result.Models[0];
        }

        public async Task<UserStory?> GetUserStoryByIdAsync(string id)
        {
            var result = await _supabaseClient.From<UserStory>()
                .Where(x => x.Id == id)
                .Get();
            return result.Models.Count > 0 ? result.Models[0] : null;
        }

        public async Task<List<UserStory>> GetAllUserStoriesAsync()
        {
            var result = await _supabaseClient.From<UserStory>()
                .Order("created_at", Postgrest.Constants.Ordering.Descending)
                .Get();
            return result.Models;
        }

        public async Task DeleteUserStoryAsync(string id)
        {
            var query = _supabaseClient.From<UserStory>()
                .Where(x => x.Id == id);
            await query.Delete();
        }
    }
}
