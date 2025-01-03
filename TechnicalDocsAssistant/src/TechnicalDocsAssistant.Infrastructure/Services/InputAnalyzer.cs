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

        private static readonly string[] PSEUDOCODE_KEYWORDS = {
            "code:", "pseudocode", "algorithm", "implementation steps", "code implementation",
            "code structure", "program structure", "solution steps"
        };

        public static bool IsUserStoryRequest(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
                
            return USER_STORY_KEYWORDS.Any(keyword =>
                input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public static bool IsPseudoCodeRequest(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
                
            return PSEUDOCODE_KEYWORDS.Any(keyword =>
                input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }
    }
}
