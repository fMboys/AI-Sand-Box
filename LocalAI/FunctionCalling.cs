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

    //Parameterized function
    [Description("Convert temperature between Celsius and Fahrenheit")]
    public static string ConvertTemperature(
        [Description("The temperature value to convert")] double value,
        [Description("The unit to convert from: 'C' for Celsius, 'F' for Fahrenheit")] string fromUnit
    )
    {
        if (fromUnit.ToUpper() == "C")
        {
            double fahrenheit = (value * 9 / 5) + 32;
            return $"{value}°C = {fahrenheit:F1}°F";
        }
        else
        {
            double celsius = (value - 32) * 5 / 9;
            return $"{value}°F = {celsius:F1}°C";
        }
    }

    //Handle errors gracefully
    [Description("Get stock price for a ticker symbol")]
    public static string GetStockPrice(string symbol)
    {
        try
        {
            // ... call stock API
            return $"{symbol}: $142.50";
        }
        catch (Exception)
        {
            return $"Unable to retrieve stock price for {symbol}. The ticker may be invalid.";
        }
    }

    [Description("Search for nearby restaurants")]
    public static string SearchRestaurants(string location, string cuisine)
    {
        // In a real app, you'd call a restaurant search API here
        return $"Found {Random.Shared.Next(5, 10)} {cuisine} restaurants in {location}.";
    }


}

