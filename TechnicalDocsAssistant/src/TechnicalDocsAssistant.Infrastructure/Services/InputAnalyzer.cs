using System;
using System.Linq;

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public static class InputAnalyzer
    {
        private static readonly string[] USER_STORY_KEYWORDS = {
            "user story", "requirements", "system changes", "new feature", "enhancement",
            "implementation", "development request", "functionality"
        };

        public static bool IsUserStoryRequest(string input)
        {
            return USER_STORY_KEYWORDS.Any(keyword =>
                input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}
