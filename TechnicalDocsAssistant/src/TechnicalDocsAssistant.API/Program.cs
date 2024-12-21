using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TechnicalDocsAssistant.Core.Interfaces;
using TechnicalDocsAssistant.Infrastructure.Services;
using System.Text.Json.Serialization;

#pragma warning disable SKEXP0010 // Disable warning about experimental features

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

// Configure Semantic Kernel with OpenAI
var modelId = builder.Configuration["OpenAI:ModelId"] 
    ?? throw new InvalidOperationException("OpenAI:ModelId is not configured");
var apiKey = builder.Configuration["OpenAI:ApiKey"]
    ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured");

// Create kernel with OpenAI text embedding generation
var kernelBuilder = Kernel.CreateBuilder()
    .AddOpenAITextEmbeddingGeneration(
        modelId: "text-embedding-ada-002", 
        apiKey: apiKey
    )
    .AddOpenAIChatCompletion(
        modelId: modelId,
        apiKey: apiKey
    );
var kernel = kernelBuilder.Build();

builder.Services.AddSingleton(kernel);

// Configure Supabase
var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase:Url is not configured");
var supabaseKey = builder.Configuration["Supabase:Key"]
    ?? throw new InvalidOperationException("Supabase:Key is not configured");

// Register Services
builder.Services.AddScoped<IUserStoryService>(sp => 
    new SupabaseUserStoryService(supabaseUrl, supabaseKey));

builder.Services.AddScoped<IAssetService>(sp => 
    new AssetService(sp.GetRequiredService<Kernel>(), supabaseUrl, supabaseKey));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();

app.Run();
