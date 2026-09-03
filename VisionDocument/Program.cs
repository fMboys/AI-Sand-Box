// Vision capable local model
using Microsoft.Extensions.AI;
using OllamaSharp;


IChatClient chatClient = new OllamaApiClient("http://127.0.0.1:11434", "llava");

var imageBytes = await File.ReadAllBytesAsync("photo.jpg");
var image = new DataContent(imageBytes, "image/jpeg");

var messages = new List<ChatMessage>
{
    new(ChatRole.User, new AIContent[]
    {
        new TextContent("What is in this image?"),
        image
    })
};

var response = await chatClient.GetResponseAsync(messages);
Console.WriteLine(response.Text);
