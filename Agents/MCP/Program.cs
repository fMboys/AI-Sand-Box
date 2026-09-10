
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OllamaSharp;

var ollamaEndpoint = new Uri("http://localhost:11434/");
var model = "llama3.2";

// 1. Connect to the public Microsoft Learn MCP server (no auth needed).
var clientTransport = new HttpClientTransport(new HttpClientTransportOptions
{
    Name = "Microsoft Learn MCP",
    Endpoint = new Uri("https://learn.microsoft.com/api/mcp")
});

await using var mcpClient = await McpClient.CreateAsync(clientTransport);

// 2. Discover the tools the server exposes.
var tools = await mcpClient.ListToolsAsync();
Console.WriteLine("Connected to the Microsoft Learn MCP server. Available tools:");

foreach( var tool in tools)
{
    Console.WriteLine($" -{tool.Name}: {tool.Description}");
}
Console.WriteLine();

// 3. Create the chat client and build a MAF agent that owns the MCP tools.
// The agent invokes the tools automatically while answering.
IChatClient chatClient = new OllamaApiClient(ollamaEndpoint, model);

AIAgent docsAgent = chatClient.AsAIAgent(
    name: "LearnDocsAgent",
    instructions: "You are a .NET documentation assistant. " +
                  "Use the Microsoft Learn tools to answer questions about .NET and AI. " +
                  "Always ground your answer in the docs and include a Microsoft Learn link.",
    tools: [.. tools]);

// 4. Ask the agent a question - it  decides  when to call the  Learn MCP tools.
const string question = "What is the latest version of Microsoft Agent Framework for C#? " +
    "Answer with the version number and a Microsoft Learn docs link.";

Console.WriteLine($"Question: {question}");
Console.WriteLine();
Console.WriteLine("Agent is thinking (it will call the MCP tools as needed)...");
Console.WriteLine();

// Stream the grounded answer so it appears token-by-token in the console. Tool calls
// happen automatically behind the scenes; the final text streams in live.

await foreach(var update in docsAgent.RunStreamingAsync(question))
{
    Console.Write(update.Text);
}
Console.WriteLine();
