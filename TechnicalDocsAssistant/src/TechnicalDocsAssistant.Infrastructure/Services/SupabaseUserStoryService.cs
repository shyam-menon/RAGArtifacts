using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Postgrest;
using TechnicalDocsAssistant.Core.Models;
using TechnicalDocsAssistant.Core.Interfaces;
using Supabase.Gotrue;
using Postgrest.Models;
using System.Text.Json;

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
            userStory.Id = Guid.NewGuid().ToString();
            userStory.CreatedAt = DateTime.UtcNow;
            userStory.UpdatedAt = DateTime.UtcNow;
            userStory.IsDeleted = false;

            var result = await _supabaseClient.From<UserStory>()
                .Insert(userStory);
            return result.Models[0];
        }

        public async Task<UserStory> UpdateUserStoryAsync(UserStory userStory)
        {
            // First verify the story exists
            var existing = await _supabaseClient.From<UserStory>()
                .Filter("id", Postgrest.Constants.Operator.Equals, userStory.Id)
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "false")
                .Get();

            if (existing.Models.Count == 0)
                throw new Exception($"User story with ID {userStory.Id} not found");

            // Update the story
            userStory.UpdatedAt = DateTime.UtcNow;

            // Create a clean object for the update
            var cleanUserStory = new UserStory
            {
                Id = userStory.Id,
                Title = userStory.Title,
                Description = userStory.Description,
                Actors = userStory.Actors,
                Preconditions = userStory.Preconditions,
                Postconditions = userStory.Postconditions,
                MainFlow = userStory.MainFlow,
                AlternativeFlows = userStory.AlternativeFlows,
                BusinessRules = userStory.BusinessRules,
                DataRequirements = userStory.DataRequirements,
                NonFunctionalRequirements = userStory.NonFunctionalRequirements,
                Assumptions = userStory.Assumptions,
                UpdatedAt = userStory.UpdatedAt,
                IsDeleted = false
            };

            var result = await _supabaseClient.From<UserStory>()
                .Filter("id", Postgrest.Constants.Operator.Equals, userStory.Id)
                .Update(cleanUserStory);

            return result.Models[0];
        }

        public async Task<UserStory?> GetUserStoryByIdAsync(string id)
        {
            var result = await _supabaseClient.From<UserStory>()
                .Filter("id", Postgrest.Constants.Operator.Equals, id)
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "false")
                .Get();
            return result.Models.Count > 0 ? result.Models[0] : null;
        }

        public async Task<List<UserStory>> GetAllUserStoriesAsync()
        {
            var result = await _supabaseClient.From<UserStory>()
                .Filter("is_deleted", Postgrest.Constants.Operator.Equals, "false")
                .Order("created_at", Postgrest.Constants.Ordering.Descending)
                .Get();
            return result.Models;
        }

        public async Task DeleteUserStoryAsync(string id)
        {
            var storyToUpdate = new UserStory
            {
                Id = id,
                IsDeleted = true,
                UpdatedAt = DateTime.UtcNow
            };

            await _supabaseClient.From<UserStory>()
                .Filter("id", Postgrest.Constants.Operator.Equals, id)
                .Update(storyToUpdate);
        }
    }
}
