using System.Reflection;
using System.Text.Json;

//call log method
WeatherLogger.Log("Lagos,Nigeria");


//Define Accepted Temperature Units
enum Units
{
    Celsius,
    Fahrenheit
}


// Create Attritube constraining it to only Properties
[AttributeUsage(AttributeTargets.Property)]
class UnitAttribute : Attribute
{
    public Units Unit { get; }
    public UnitAttribute(Units unit = Units.Celsius)
    {
        Unit = unit;
    }
}

//Creating Weather Class
class Weather
{
    public string country { get; set; }
    public string region { get; set; }
    //Applying the Attribute to the Temperature Property
    [Unit(Units.Fahrenheit)]
    public double Temperature { get; set; }

}

class WeatherLogger
{
    public static string apiKey = "c59654e9053c4be586d212935223101";
    public static void Log(string city)
    {
        //Trying to obtain the value Defined in the Unit Attribute  from the Temperature Property
        PropertyInfo propertyInfo = typeof(Weather).GetProperty(nameof(Weather.Temperature));
        UnitAttribute  unitAttribute = (UnitAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(UnitAttribute));
        unitAttribute ??= new UnitAttribute();
        var client = new HttpClient();
        HttpResponseMessage response = client.GetAsync($"https://api.weatherapi.com/v1/current.json?q={city}&key={apiKey}").Result;
        var info = response.Content.ReadAsStringAsync().Result;
        var country = JsonDocument.Parse(info).RootElement.GetProperty("location").GetProperty("country").ToString();
        var region = JsonDocument.Parse(info).RootElement.GetProperty("location").GetProperty("region").ToString();
        var tempC = Convert.ToDouble(JsonDocument.Parse(info).RootElement.GetProperty("current").GetProperty("temp_c").ToString());
        var tempF = Convert.ToDouble(JsonDocument.Parse(info).RootElement.GetProperty("current").GetProperty("temp_f").ToString());
       
        var newWeather = JsonSerializer.Serialize<Weather>(new Weather()
        {
            region = region,
            country = country,
            //Deciding which temperature unit to display based on the set unitattribute value
            Temperature = unitAttribute.Unit == Units.Celsius ? tempC : tempF,
        });
        Console.WriteLine(newWeather);
         
    }

}
