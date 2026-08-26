using Microsoft.Extensions.AI;
using OllamaSharp;

// start the ollama local server with the following command: ollama serve 
// Then run this program to interact with the model
IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "phi3:mini");

// Start the conversation with context for the AI model
List<ChatMessage> chatHistory = new ();

#region Streaming Chat Example
while (true)
{
    // Get user prompt and add to chat history
    Console.WriteLine("Enter prompt: ");
    var userPrompt = Console.ReadLine();
    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

// Streaming lets you display tokens as they arrive, creating a more responsive experience, like what you see in ChatGPT or Copilot.
// Stream the AI response and add to chat history
    Console.WriteLine("AI response: ");
    var response = "";
    await foreach (ChatResponseUpdate item in chatClient.GetStreamingResponseAsync(chatHistory))
    {
        Console.Write(item.Text);
        response += item.Text;
    }
    chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
    Console.WriteLine();
}
#endregion