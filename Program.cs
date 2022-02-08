using Microsoft.Extensions.Configuration;
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
    public static string apiKey = new ConfigurationBuilder().AddSamFile().Build()["secretKey"];
    public static void Log(string city)
    {
        PropertyInfo propertyInfo = typeof(Weather).GetProperty(nameof(Weather.Temperature));
        UnitAttribute unitAttribute = (UnitAttribute)propertyInfo.GetCustomAttribute(typeof(UnitAttribute));
        //UnitAttribute unitAttribute = (UnitAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(UnitAttribute));
        unitAttribute ??= new UnitAttribute();
        string country, region;
        double tempC, tempF;
        RequestData(city, out country, out region, out tempC, out tempF);
        var newWeather = JsonSerializer.Serialize(new Weather()
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



class MyProvider : ConfigurationProvider
{
    string FilePath;


    public MyProvider(string Path)
    {
        FilePath = Path;
    }


    public override void Load()
    {
        var lines = File.ReadAllLines(FilePath);
        foreach (var data in lines)
        {
            (string, string) val = (data.Split(' ').ToList()[0], data.Split(' ')[1]);
            Set(val.Item1, val.Item2);
        }
    }




}
class MySource : IConfigurationSource
{
    string FilePath;
    public MySource(string Path)
    {
        FilePath = Path;
    }
    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {

        return new MyProvider(FilePath);
    }

}

static class ExtensionClass
{
    public static IConfigurationBuilder AddSamFile(this IConfigurationBuilder builder)
    {
        return builder.Add(new MySource(@"C:\Users\Swacblooms\source\repos\AttributesCode\myconfig.sam"));
    }
}
