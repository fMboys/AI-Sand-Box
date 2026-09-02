using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OllamaSharp;
using OpenAI;

var ollamaEndpoint = new Uri("http://127.0.0.1:11434/");
var chatModelName = "llama3.2";
var embeddingModelName = "nomic-embed-text:latest";
string key = "Ollama";

// var ollamaClient = new OllamaApiClient("http://127.0.0.1:11434");
// IChatClient chatClient = ollamaClient.SelectedModel.

// Embedding generator for retrieval 
IEmbeddingGenerator<string, Embedding<float>> generator = new OpenAIClient(new ApiKeyCredential(key),
    new OpenAIClientOptions
    {
        Endpoint = ollamaEndpoint
    }).GetEmbeddingClient(embeddingModelName).AsIEmbeddingGenerator();
// IEbeddingGenerator<string, Embedding<float>> generator = ollamaClient.As

VectorStore vectorStore = new InMemoryVectorStore();
var collection = vectorStore.GetCollection<string, Document>("docs");
await collection.EnsureCollectionExistsAsync();

// Add documents to the collection
var documents = new[]
{
    new { Id = "policy-1", Title = "Refund Policy", 
          Category = "Policies",
          Content = "Software purchases can be refunded within 30 days of purchase with proof of receipt. Digital downloads are non-refundable once activated." },
    
    new { Id = "policy-2", Title = "Hardware Warranty", 
          Category = "Policies",
          Content = "All hardware products come with a 2-year warranty covering manufacturing defects. Extended warranties are available for purchase." },
    
    new { Id = "faq-1", Title = "Account Recovery", 
          Category = "FAQ",
          Content = "To recover your account, click 'Forgot Password' on the login page. A reset link will be sent to your registered email within 5 minutes." },
    
    // Add more documents...
};

foreach (var doc in documents)
{
    var embedding = await generator.GenerateVectorAsync(doc.Content);
    await collection.UpsertAsync( new Document
        {
            Id = doc.Id,
            Title = doc.Title,
            Category = doc.Category,
            Content = doc.Content,
            Embedding = embedding
        });
}

Console.WriteLine($"Indexed {documents.Length} documents into the vector store.");

async Task<List<Document>> RetrievelAsync(string query, int topK = 3)
{
    
}