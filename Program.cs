using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Reflection;
using System.Text.Json;

WeatherLogger.Log("Lagos,Nigeria");


enum Units
{
    Celsius,
    Fahrenheit
}


[AttributeUsage(AttributeTargets.Property)]
class UnitAttribute : Attribute
{
    public Units Unit { get; }
    public UnitAttribute(Units unit = Units.Celsius)
    {
        Unit = unit;
    }
}


class Weather
{
    public string country { get; set; }
    public string region { get; set; }
    [Unit(Units.Fahrenheit)]
    public double Temperature { get; set; }

}


class WeatherLogger
{
    public static string apiKey = new ConfigurationBuilder().AddJsonFile("myconfig.json").Build()["secretKey"];


   public static void Log(string city)
    {
        PropertyInfo propertyInfo = typeof(Weather).GetProperty(nameof(Weather.Temperature));
        UnitAttribute unitAttribute = (UnitAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(UnitAttribute));
        unitAttribute ??= new UnitAttribute();
        string country, region;
        double tempC, tempF;
        RequestData(city, out country, out region, out tempC, out tempF);
        var newWeather = JsonSerializer.Serialize<Weather>(new Weather()
        {
            region = region,
            country = country,
            Temperature = unitAttribute.Unit == Units.Celsius ? tempC : tempF,
        });
        Console.WriteLine(newWeather);


    }

    private static void RequestData(string city, out string country, out string region, out double tempC, out double tempF)
    {
        var client = new HttpClient();
        HttpResponseMessage response = client.GetAsync($"https://api.weatherapi.com/v1/current.json?q={city}&key={apiKey}").Result;
        var info = response.Content.ReadAsStringAsync().Result;
        country = JsonDocument.Parse(info).RootElement.GetProperty("location").GetProperty("country").ToString();
        region = JsonDocument.Parse(info).RootElement.GetProperty("location").GetProperty("region").ToString();
        tempC = Convert.ToDouble(JsonDocument.Parse(info).RootElement.GetProperty("current").GetProperty("temp_c").ToString());
        tempF = Convert.ToDouble(JsonDocument.Parse(info).RootElement.GetProperty("current").GetProperty("temp_f").ToString());
    }
}
