using System.ClientModel;
using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;

var ollamaEndpoint = new Uri("http://127.0.0.1:11434/v1/");
var chatModelName = "llama3.2";
var embeddingModelName = "nomic-embed-text:latest";
string key = "Ollama";

IChatClient chatClient = new OllamaApiClient(new Uri("http://127.0.0.1:11434"), chatModelName);

// Embedding generator for retrieval 
IEmbeddingGenerator<string, Embedding<float>> generator = new OpenAIClient(new ApiKeyCredential(key),
    new OpenAIClientOptions
    {
        Endpoint = ollamaEndpoint
    }).GetEmbeddingClient(embeddingModelName).AsIEmbeddingGenerator();
// IEbeddingGenerator<string, Embedding<float>> generator = ollamaClient.As

var vectorStore = new InMemoryVectorStore();
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
    var queryEmbedding = await generator.GenerateVectorAsync(query);
    var results = collection.SearchAsync(queryEmbedding, top: topK);

    var documents = new List<Document>();
    await foreach (var result in results)
    {
        documents.Add(result.Record);
    }
    return documents;
}

string BuildPrompt(string question, List<Document> context)
{
    string contextText = string.Join("\n\n", context.Select(d => 
    $"### {d.Title}\n{d.Content}"));

    return $"""
        You are a helpful assistant. Answer the user's question using ONLY the 
        information provided in the context below. If the answer is not in the 
        context, say "I don't have information about that."
        
        ## Context
        {contextText}
        
        ## Question
        {question}
        """;
}

async Task<string> AskAysnc(string question)
{
    // 1. Retrieve relevant documents
    var relevantDocs = await RetrievelAsync(question);

    Console.WriteLine($"Found {relevantDocs.Count} relevant documents:");
    foreach (var doc in relevantDocs)
    {
        Console.WriteLine($" - {doc.Title}");
    }

    // 2. Build augmented prompt with context
    var prompt = BuildPrompt(question, relevantDocs);

    // 3. Generate response
    var response = await chatClient.GetResponseAsync(prompt);

    return response.Text;
}

// Ask questions against your knowledge base
var questions = new[]
{
    "Can I get a refund on my software purchase?",
    "How long is the hardware warranty?",
    "I forgot my password, what should I do?"
};

//TODO
// // Be expilicit about using only the context
// var systemPrompt = """
//     You are a customer support assistant. Your job is to help users by answering 
//     their questions base on our company documentation.

//     RULES:
//     1. Only use information from the provided context
//     2. If the answer is not in the context, say so clearly
//     3. Quote relevant parts of the documentation if possible
//     4. Be concise but complete
//     """;

// List<ChatMessage> chatHistory = new();
// chatHistory.Add(new ChatMessage(ChatRole.System, systemPrompt));

foreach (var question in questions)
{
    Console.WriteLine($"\nQ: {question}");
    var answer = await AskAysnc(question);
    
    Console.WriteLine($"A: {answer}");
    Console.WriteLine(new string('-', 50));
}