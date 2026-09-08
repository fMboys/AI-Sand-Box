using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;

public class WeatherService
{
    private readonly HttpClient _httpClient = new();

    [Description("Get real weather data for a city")]
    public async Task<string> GetRealWeather([Description("City name")] string city)
    {
        try
        {
            //Using a free weather api
            var url = $"https://wttr.in/{city}?format=j1";
            var data = await _httpClient.GetFromJsonAsync<JsonElement>(url);

            var temp = data.GetProperty("current_condition")[0].GetProperty("temp_C").GetString();
            var desc = data.GetProperty("current_condition")[0]
                .GetProperty("weatherDesc")[0]
                .GetProperty("value").GetString();

            return $"Current weather in {city}: {desc}, {temp}°C";
        }
        catch
        {
            return $"Sorry, couldn't get weather data for {city}.";
        }
    }
}