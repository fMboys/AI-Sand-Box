using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenTelemetry.Trace;
using OllamaSharp;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


var ollamaEndpoint = new Uri("http://127.0.0.1:11434/");
string chatModelName = "llama3.2";
var sourceName = "LocalAI";

#region Local
//// Create the cache
//var cache = new MemoryDistributedCache(
//    Options.Create(new MemoryDistributedCacheOptions()));

//// Configure Redis cache (if you want to use Redis instead of in-memory cache)
//var redis = new RedisCache(new RedisCacheOptions
//{
//    Configuration = "localhost:6379"
//});

//// Configure OpenTelemetry
//var tracerProvider = OpenTelemetry.Sdk.CreateTracerProviderBuilder()
//    .AddSource(sourceName)
//    .AddConsoleExporter()           // Or AddOtlpExporter() for production
//    .Build();

//// Build the chat client with middleware
//IChatClient chatClient = ((IChatClient)new OllamaApiClient(ollamaEndpoint, chatModelName))
//    .AsBuilder()                    // Start building the pipeline
//    .UseFunctionInvocation()        // Add function calling
//    .UseDistributedCache(cache)     // Add caching
//    //.UseOpenTelemetry(tracerProvider)   // Add telemetry
//    .Build();                       // Create the final client


//string[] prompts = ["What is AI?", "What is .NET?", "What is AI?"];

//foreach (var prompt in prompts)
//{
//    Console.WriteLine($"Prompt: {prompt}");
//    var response = await chatClient.GetResponseAsync(prompt);
//    Console.WriteLine($"Response: {response.Text}\n");
//}

#endregion

// --------- Production Ready & DI Example -----------

var builder = Host.CreateApplicationBuilder();

// Configure services
builder.Services.AddDistributedMemoryCache(); // Use in-memory cache
builder.Services.AddLogging(l => l.AddConsole()); // Add console logging

// Configure OpenTelemetry
//builder.Services.AddOpenTelemetry().WithTracing(
//    tracing => tracing.AddSource(sourceName).AddConsoleExporter());

// Register the AI client with full middleware pipeline
builder.Services.AddChatClient(services =>
{
    var cache = services.GetRequiredService<IDistributedCache>();
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();

    return ((IChatClient)new OllamaApiClient(ollamaEndpoint, chatModelName)).AsBuilder()
    .ConfigureOptions(o => o.Temperature = 0.7f)
    .UseDistributedCache(cache)
    .UseFunctionInvocation()
    .UseLogging(loggerFactory)
    //.UseOpenTelemetry(sourceName: "localai")
    .Build();
});

var host = builder.Build();
var chatClient = host.Services.GetRequiredService<IChatClient>();

// Use production ready client
Console.WriteLine("Production Ready Client!");
var response = await chatClient.GetResponseAsync("What is .NET?");
Console.WriteLine($"{response.Text}");
