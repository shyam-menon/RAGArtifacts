//Code that selects between RAG and user story
public class Program
{
    
    // Replace with your actual API key
    private static string? OpenAIApiKey => Environment.GetEnvironmentVariable("OPENAI_API_KEY");

    public static async Task Main()
    {
        // Initialize services
        #pragma warning disable SKEXP0010
        var embeddingGeneration = new OpenAITextEmbeddingGenerationService(
            modelId: "text-embedding-ada-002",
            apiKey: OpenAIApiKey);

        // Create kernel with chat completion service
        var kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion("gpt-4o-mini", OpenAIApiKey)
            .Build();

        // Initialize vector store and collection
        var vectorStore = new InMemoryVectorStore();
        var collection = vectorStore.GetCollection<string, Document>("documents");
        await collection.CreateCollectionIfNotExistsAsync();

        // Load documents from markdown files
        Console.WriteLine("RAG with Open AI and InMemory vector store. Loading markdown files...");
        var markdownPath = Path.Combine(Directory.GetCurrentDirectory(), "docs"); // Folder containing your markdown files
        var documents = await MarkdownFileReader.ReadMarkdownFilesFromDirectory(markdownPath);
       

        // Ingest documents
        Console.WriteLine("Ingesting documents...");
        foreach (var doc in documents)
        {
            var embedding = await embeddingGeneration.GenerateEmbeddingAsync(doc.Content);

            await collection.UpsertAsync(new Document
            {
                Id = Guid.NewGuid().ToString(),
                Title = doc.Title,
                Content = doc.Content,
                ContentEmbedding = embedding
            });
        }

        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var userStoryAgent = new UserStoryAgent(chatCompletionService, collection, embeddingGeneration);

        // Create a search function that we'll use in our RAG prompt
        var searchFunction = async (string query) =>
        {
            var queryEmbedding = await embeddingGeneration.GenerateEmbeddingAsync(query);
            var searchResults = await collection.VectorizedSearchAsync(queryEmbedding, new() { Top = 2 });

            var results = new List<string>();
            await foreach (var result in searchResults.Results)
            {
                results.Add($"Title: {result.Record.Title}\nContent: {result.Record.Content}\n");
            }

            return string.Join("\n", results);
        };

        // Create chat history
        var chatHistory = new ChatHistory();

        while (true)
        {
            // Get user input
            Console.Write("\nEnter your question (or 'exit' to quit): ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                break;

            if (InputAnalyzer.IsUserStoryRequest(input))
            {
                var userStory = await userStoryAgent.GenerateUserStory(input);
                await File.WriteAllTextAsync($"UserStory_{DateTime.Now:yyyyMMddHHmmss}.md", userStory);
                Console.WriteLine("\nGenerated User Story:\n" + userStory);
            }
            else
            {
                var response = await ProcessRagQuery(input, collection, embeddingGeneration, chatCompletionService);
                Console.WriteLine("\nResponse: " + response);               

                // Add AI response to chat history
                chatHistory.AddAssistantMessage(response);
            }

           

            
        }
    }

    private static async Task<string> ProcessRagQuery(
   string query,
   IVectorStoreRecordCollection<string, Document> collection,
   ITextEmbeddingGenerationService embeddingGeneration,
   IChatCompletionService chatCompletionService)
    {
        // Generate embedding for query
        var queryEmbedding = await embeddingGeneration.GenerateEmbeddingAsync(query);

        // Search for relevant documents
        var searchResults = await collection.VectorizedSearchAsync(queryEmbedding, new() { Top = 2 });

        // Build context from search results
        var contextBuilder = new StringBuilder();
        await foreach (var result in searchResults.Results)
        {
            contextBuilder.AppendLine($"Content: {result.Record.Content}\n");
        }

        // Create RAG prompt
        var prompt = $$"""
       Use the following information to answer the question. 
       If you cannot answer based on the provided information, say "I don't have enough information to answer that question."

       Context:
       {{contextBuilder}}

       Question: {{query}}

       Answer:
       """;

        // Get AI response
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(prompt);
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory);

        return response.Content;
    }

    // User story agent
    using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable SKEXP0001

namespace SK_RAGOpenAI
{
    public class UserStoryAgent
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IVectorStoreRecordCollection<string, Document> _collection;
        private readonly ITextEmbeddingGenerationService _embeddingService;

        public UserStoryAgent(
            IChatCompletionService chatCompletionService,
            IVectorStoreRecordCollection<string, Document> collection,
            ITextEmbeddingGenerationService embeddingService)
        {
            _chatCompletionService = chatCompletionService;
            _collection = collection;
            _embeddingService = embeddingService;
        }

        public async Task<string> GenerateUserStory(string input)
        {
            // Search for relevant context
            var searchEmbedding = await _embeddingService.GenerateEmbeddingAsync(input);
            var searchResults = await _collection.VectorizedSearchAsync(searchEmbedding, new() { Top = 3 });

            var contextBuilder = new StringBuilder();
            await foreach (var result in searchResults.Results)
            {
                contextBuilder.AppendLine(result.Record.Content);
            }

            var prompt = $@"Use the following reference information and input to create a detailed user story in markdown format.
Follow the exact structure and formatting from the reference information.
Include all sections as shown in the reference: User Story ID/Title, Description, Actors, Preconditions, Postconditions, Main Flow, Alternative Flows, Business Rules, Data Requirements, Non-functional Requirements, and Assumptions/Dependencies.

Reference Information:
{contextBuilder}

Input:
{input}

Generate the user story:";

            var chatHistory = new ChatHistory();
            chatHistory.AddSystemMessage(prompt);

            var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory);
            return response.Content;
        }
    }
}

//InputAnalyzer - Analyses the type of chat request

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SK_RAGOpenAI
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
                input.Contains(keyword, StringComparison.OrdinalIgnoreCase)) /*&&
                input.Contains("system", StringComparison.OrdinalIgnoreCase)*/;
        }
    }
}

