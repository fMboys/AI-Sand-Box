using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;

WeatherService weatherService = new WeatherService();
IChatClient chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2");

AIAgent agent = chatClient.AsAIAgent(
    name: "WeatherBot",
    instructions: "You provide accurate weather information.",
    tools: [AIFunctionFactory.Create(weatherService.GetRealWeather)]);

    var response = await agent.RunAsync("What is the weather in Paris?");
    Console.WriteLine(response.Text);

