using System;
using System.Linq;

namespace TechnicalDocsAssistant.Infrastructure.Services
{
    public static class InputAnalyzer
    {
        /*private static readonly string[] USER_STORY_KEYWORDS = {
            "user story", "requirements", "system changes", "new feature", "enhancement",
            "implementation", "development request", "functionality"
        };*/

        private static readonly string[] USER_STORY_KEYWORDS = {
            "user story", "story"
        };

        /*private static readonly string[] PSEUDOCODE_KEYWORDS = {
            "code:", "pseudocode", "algorithm", "implementation steps", "code implementation",
            "code structure", "program structure", "solution steps"
        };*/

        private static readonly string[] PSEUDOCODE_KEYWORDS = {
            "code:", "pseudocode"
        };

        private static readonly string[] SERVICE_TICKET_KEYWORDS = {
            "service ticket"
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

        public static bool IsServiceTicketRequest(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
                
            return SERVICE_TICKET_KEYWORDS.Any(keyword =>
                input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public static string GetServiceTicketQuery(string userMessage)
        {
            return $@"Find similar support tickets and their resolutions for:
{userMessage}
Based on this information:
1. Provide resolution steps from similar cases
2. List relevant MSSI ticket numbers and their resolutions
3. Identify common solutions for this type of issue
4. Suggest troubleshooting steps based on historical tickets";
        }
    }
}
