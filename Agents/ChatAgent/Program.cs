using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

// var config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

IChatClient client = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2");
// Create an agent
AIAgent assistant = client.AsAIAgent(
    name: "Assistant",
    instructions: "You are helpful assistant that remembers our conversation.");

// Create a thread to maintain context
AgentSession session = await assistant.CreateSessionAsync();

// First message
var response1 = await assistant.RunAsync("My name is Name.", session);
Console.WriteLine($"Agent: {response1.Text}");

// Second message
var response2 = await assistant.RunAsync("What is my name?", session);
Console.WriteLine($"Agents: {response2.Text}");

// Second message
var response3 = await assistant.RunAsync("Can you spell it backwards?", session);
Console.WriteLine($"Agents: {response3.Text}");