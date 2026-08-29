using Microsoft.Extensions.AI;
using OllamaSharp;
using OllamaSharp.Models.Chat;
using System;
using System.ComponentModel;

// start the ollama local server with the following command: ollama serve 
// Then run this program to interact with the model
// var ollamaEndpoint = new Uri("http://localhost:11434/");
var ollamaEndpoint = new Uri("http://127.0.0.1:11434/");
var chatModelname = "llama3.2";
IChatClient chatClient = new OllamaApiClient(ollamaEndpoint, chatModelname);

// Start the conversation with context for the AI model
List<ChatMessage> chatHistory = new ();

#region Streaming Chat Example
// while (true)
// {
//     // Get user prompt and add to chat history
//     Console.WriteLine("Enter prompt: ");
//     var userPrompt = Console.ReadLine();
//     chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

// // Streaming lets you display tokens as they arrive, creating a more responsive experience, like what you see in ChatGPT or Copilot.
// // Stream the AI response and add to chat history
//     Console.WriteLine("AI response: ");
//     var response = "";
//     await foreach (ChatResponseUpdate item in chatClient.GetStreamingResponseAsync(chatHistory))
//     {
//         Console.Write(item.Text);
//         response += item.Text;
//     }
//     chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
//     Console.WriteLine();
// }
#endregion

#region Structured output example

// string review = "I'm happy with the product!";
// var response = await chatClient.GetResponseAsync<Sentiment>($"What's the sentiment of this review? {review}");
// Console.WriteLine($"Sentiment: {response.Result}");
// Output: Sentiment: Positive

// Analyzing Multiple Items
// string[] reviews = [
//     "Best purchase ever!",
//     "Returned it immediately.",
//     "Hello",
//     "It works as advertised.",
//     "The packaging was damaged but otherwise okay."
// ];

// foreach (var view in reviews)
// {
//     var res = await chatClient.GetResponseAsync<Sentiment>(
//         $"What's the sentiment of this review? {view}");
//         Console.WriteLine($"Review: {view} | Sentiment: {res.Result}");
// }
// public enum Sentiment
// {
//     Positive,
//     Negative,
//     Neutral
// }

#endregion
#region Function Calling Example

chatClient = chatClient.AsBuilder().UseFunctionInvocation().Build();

// var chatOptions = new ChatOptions
// {
//     Tools = [AIFunctionFactory.Create(FunctionCalling.GetWeather)]
// };

//Calling multiple functions
var chatOptions = new ChatOptions
{
    Tools = [
        AIFunctionFactory.Create(FunctionCalling.GetWeather),
        AIFunctionFactory.Create(FunctionCalling.ConvertTemperature),
        AIFunctionFactory.Create(FunctionCalling.GetStockPrice),
    ]
};

var response = await chatClient.GetResponseAsync("Shoult I bring an umbrella to New York City today?", chatOptions);
Console.WriteLine(response.Text);

// Calls GetStockPrice
await chatClient.GetResponseAsync("How is Microsoft stock doing?", chatOptions);

// Calls SearchRestaurants
await chatClient.GetResponseAsync("Find Italian restaurants near downtown Seattle", chatOptions);

// Might call multiple functions
await chatClient.GetResponseAsync(
    "I'm visiting Paris tomorrow. What's the weather like, and can you suggest some good cafes?", 
    chatOptions);
#endregion