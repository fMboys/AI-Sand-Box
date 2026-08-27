using System.ComponentModel;

public static class FunctionCalling
{
    [Description("Get the current weather for a given location")]
    public static string GetWeather(string location)
    {
        // In a real app, you'd call a weather API here
        var temperature = Random.Shared.Next(15, 30);
        var conditions = Random.Shared.Next(0, 2) == 0 ? "sunny" : "cloudy";
        return $"{location}: {temperature}°C and {conditions}";
    }
}