using System.ClientModel;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;


// var groqEndpoint = "https://api.groq.com/openai/v1";

var ollamaEndpoint = new Uri("http://127.0.0.1:11434/v1/");
var chatModelName = "nomic-embed-text:latest";
string ok = "Ollama";

// Generate embeddings directly with the Ollama client
IEmbeddingGenerator<string, Embedding<float>> generator = new OpenAIClient(new ApiKeyCredential(ok),
    new OpenAIClientOptions
    {
        Endpoint = ollamaEndpoint
    }).GetEmbeddingClient(chatModelName).AsIEmbeddingGenerator();
    
// Generate an embedding for text
var embedding = await generator.GenerateAsync("I love pizza");

// The result is a vector of floats
Console.WriteLine($"Embedding dimensions: {embedding.Vector.Length}");

// Embeding multiple items
string[] documents = [
    "The quick brown fox jumps over the lazy dog",
    "A fast auburn fox leaps above a sleepy canine",
    "The weather is nice today",
    "I enjoy programming in C#"
];

var embeddings = await generator.GenerateAsync(documents);

foreach(var (doc, emb) in documents.Zip(embeddings))
{
    Console.WriteLine($"'{doc.Substring(0,20)}...' {emb.Vector.Length} dimensions");
}

// Finding similar sentences using cosine similarity
var query = "athletic footwear for running";
var queryEmbedding = await generator.GenerateVectorAsync(query);

string[] products = [
    "Running shoes for marathon training",
    "Comfortable sneakers for jogging",
    "Leather dress shoes for formal occasions",
    "Hiking boots for mountain trails",
    "Basketball shoes for indoor courts"
];

var productEmbeddings = await generator.GenerateAsync(products);

// Calculate cosine similarity between the query and each product
var results = products.Zip(productEmbeddings).Select(p => new
{
    Product = p.First,
    Similarity = CosineSimilarity(queryEmbedding, p.Second.Vector)
}).OrderByDescending(r => r.Similarity).ToList();

Console.WriteLine($"Query: '{query}\n'");
foreach ( var result in results)
{
    Console.WriteLine($"{result.Similarity:F3} - {result.Product}");
}

// Create the vector store and collection
var vectorStore = new InMemoryVectorStore();
var productCollection = vectorStore.GetCollection<int, Product>("products");
await productCollection.EnsureCollectionExistsAsync();

// Your product data
var productData = new[]
{
    new { Id = 1, Name = "Trail Runner Pro", Description = "Lightweight running shoes for trail running" },
    new { Id = 2, Name = "Urban Jogger", Description = "Comfortable sneakers for city jogging" },
    new { Id = 3, Name = "Executive Oxford", Description = "Classic leather dress shoes for business" },
    // ... more products
};

// Generate embeddings and store
foreach(var p in productData)
{
   var embedd = await generator.GenerateVectorAsync(p.Description);
    await productCollection.UpsertAsync(new Product
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Embedding = embedd
    });
}

// Search for similar products
var searchQuery = "shoes for my morning jog";
var queryEmbeddings = await generator.GenerateVectorAsync(searchQuery);

var searchResult = productCollection.SearchAsync(queryEmbeddings, 3);

Console.WriteLine($"Results for: '{searchQuery}'\n");
await foreach(var result in searchResult)
{
    Console.WriteLine($"Score: {result.Score:F3}");
    Console.WriteLine($"Product: {result.Record.Name}");
    Console.WriteLine($"Description: {result.Record.Description}\n");
}



float CosineSimilarity(ReadOnlyMemory<float> a, ReadOnlyMemory<float> b)
{
    var spanA = a.Span;
    var spanB = b.Span;

    float dotProduct = 0, normA = 0, normB = 0;

    for (int i= 0; i < spanA.Length; i++)
    {
        dotProduct += spanA[i] * spanB[i];
        normA += spanA[i] * spanA[i];
        normB += spanB[i] * spanB[i];
    }

    return dotProduct / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
}
