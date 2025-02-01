using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Infrastructure.Services;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel.Embeddings;

#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0020 // API is for evaluation purposes only and is subject to change or removal in future updates.
#pragma warning disable SKEXP0050 // API is for evaluation purposes only and is subject to change or removal in future updates.

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.IgnoreReadOnlyFields = true;
        options.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    });
builder.Services.AddEndpointsApiExplorer();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configure Semantic Kernel with Azure OpenAI
var chatCompletionDeploymentName = builder.Configuration["AzureOpenAI:ChatCompletionDeploymentName"] 
    ?? throw new InvalidOperationException("AzureOpenAI:ChatCompletionDeploymentName is not configured");
var embeddingDeploymentName = builder.Configuration["AzureOpenAI:EmbeddingDeploymentName"]
    ?? throw new InvalidOperationException("AzureOpenAI:EmbeddingDeploymentName is not configured");

// Get Azure OpenAI credentials from environment variables
var endpoint = Environment.GetEnvironmentVariable("AZURE_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_ENDPOINT environment variable is not set");
var apiKey = Environment.GetEnvironmentVariable("AZURE_API_KEY")
    ?? throw new InvalidOperationException("AZURE_API_KEY environment variable is not set");

// Create kernel with Azure OpenAI text embedding generation and chat completion
var kernelBuilder = Kernel.CreateBuilder()
    .AddAzureOpenAITextEmbeddingGeneration(
        deploymentName: embeddingDeploymentName,
        endpoint: endpoint,
        apiKey: apiKey
    )
    .AddAzureOpenAIChatCompletion(
        deploymentName: chatCompletionDeploymentName,
        endpoint: endpoint,
        apiKey: apiKey
    );
var kernel = kernelBuilder.Build();

// Get the embedding service from the kernel
var embeddingService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

builder.Services.AddSingleton(kernel);
builder.Services.AddSingleton<ITextEmbeddingGenerationService>(embeddingService);

// Configure Supabase
var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase:Url is not configured");
var supabaseKey = builder.Configuration["Supabase:Key"]
    ?? throw new InvalidOperationException("Supabase:Key is not configured");

// Create Supabase client
var supabaseOptions = new Supabase.SupabaseOptions
{
    AutoRefreshToken = true,
    AutoConnectRealtime = true
};
var supabaseClient = new Supabase.Client(supabaseUrl, supabaseKey, supabaseOptions);

// Register Services
builder.Services.AddScoped<IUserStoryService>(sp => 
    new SupabaseUserStoryService(supabaseUrl, supabaseKey));

builder.Services.AddScoped<IAssetService>(sp => 
    new AssetService(supabaseClient, sp.GetRequiredService<Kernel>()));

builder.Services.AddScoped<IChatService, SimpleChatService>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
