using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.SemanticKernel;
using TechnicalDocsAssistant.Core.Models;

namespace TechnicalDocsAssistant.SKPlugins
{
    public class ArtifactGenerationPlugin
    {
        private readonly Kernel _kernel;

        public ArtifactGenerationPlugin(Kernel kernel)
        {
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        [KernelFunction]
        [Description("Generates a PlantUML flowchart from a user story")]
        public async Task<string> GenerateFlowchart(string userStoryJson)
        {
            if (string.IsNullOrEmpty(userStoryJson))
                throw new ArgumentNullException(nameof(userStoryJson));

            var userStory = JsonSerializer.Deserialize<UserStory>(userStoryJson) 
                ?? throw new ArgumentException("Invalid user story JSON", nameof(userStoryJson));
            
            // Create a prompt for flowchart generation
            var prompt = @$"
Create a PlantUML flowchart for the following user story:
Title: {userStory.Title}
Description: {userStory.Description}
Main Flow: {userStory.MainFlow}
Alternative Flows: {string.Join(", ", userStory.AlternativeFlows ?? new List<string>())}

Generate a PlantUML flowchart that:
1. Shows the main flow as a sequence of steps
2. Includes decision points for alternative flows
3. Uses appropriate PlantUML syntax and symbols
4. Is clear and easy to understand

Output only the PlantUML code without any explanations.";

            // Use the kernel to generate the flowchart
            var result = await _kernel.InvokePromptAsync(prompt);
            return result?.ToString() ?? "Error generating flowchart";
        }

        [KernelFunction]
        [Description("Generates a sequence diagram from a user story")]
        public async Task<string> GenerateSequenceDiagram(string userStoryJson)
        {
            if (string.IsNullOrEmpty(userStoryJson))
                throw new ArgumentNullException(nameof(userStoryJson));

            var userStory = JsonSerializer.Deserialize<UserStory>(userStoryJson)
                ?? throw new ArgumentException("Invalid user story JSON", nameof(userStoryJson));
            
            // Create a prompt for sequence diagram generation
            var prompt = @$"
Create a PlantUML sequence diagram for the following user story:
Title: {userStory.Title}
Description: {userStory.Description}
Actors: {string.Join(", ", userStory.Actors ?? new List<string>())}
Main Flow: {userStory.MainFlow}

Generate a PlantUML sequence diagram that:
1. Shows interactions between actors and systems
2. Represents the main flow as a sequence of messages
3. Uses appropriate PlantUML syntax and symbols
4. Is clear and easy to understand

Output only the PlantUML code without any explanations.";

            // Use the kernel to generate the sequence diagram
            var result = await _kernel.InvokePromptAsync(prompt);
            return result?.ToString() ?? "Error generating sequence diagram";
        }

        [KernelFunction]
        [Description("Generates test cases from a user story")]
        public async Task<string> GenerateTestCases(string userStoryJson)
        {
            if (string.IsNullOrEmpty(userStoryJson))
                throw new ArgumentNullException(nameof(userStoryJson));

            var userStory = JsonSerializer.Deserialize<UserStory>(userStoryJson)
                ?? throw new ArgumentException("Invalid user story JSON", nameof(userStoryJson));
            
            // Create a prompt for test case generation
            var prompt = @$"
Create test cases for the following user story:
Title: {userStory.Title}
Description: {userStory.Description}
Preconditions: {string.Join(", ", userStory.Preconditions ?? new List<string>())}
Main Flow: {userStory.MainFlow}
Alternative Flows: {string.Join(", ", userStory.AlternativeFlows ?? new List<string>())}
Business Rules: {string.Join(", ", userStory.BusinessRules ?? new List<string>())}

Generate test cases that:
1. Cover the main flow
2. Include test cases for alternative flows
3. Validate business rules
4. Test edge cases and error conditions

Output the test cases in markdown format using the following template for each test case:
### Test Case [ID]
**Description**: [Brief description]
**Preconditions**: [List of preconditions]
**Steps**:
1. [Step 1]
2. [Step 2]
...
**Expected Result**: [Expected outcome]
";

            // Use the kernel to generate the test cases
            var result = await _kernel.InvokePromptAsync(prompt);
            return result?.ToString() ?? "Error generating test cases";
        }
    }
}
