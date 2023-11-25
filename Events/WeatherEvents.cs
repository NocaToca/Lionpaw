

//This class handles all of our weather events
using System.DirectoryServices.ActiveDirectory;
using DSharpPlus.Entities;
using Lionpaw;
using Newtonsoft.Json;

public class WeatherEventManager{

    public struct WeatherData{
        [JsonProperty("time")]
        public long time;
        [JsonConstructor]
        public WeatherData(long time){
            this.time = time;
        }
    }

    WeatherEvent current_event;

    Bot bot;
    DiscordGuild guild;
    Thread thread;
    public WeatherEventManager(Bot bot, DiscordGuild guild){
        this.bot = bot;
        this.guild = guild;
        thread = new Thread(MainThread);
        thread.Start();
    }

    //Our main weatherevents thread
    void MainThread(){
        while(true){
            // Logger.Log("Creating Event");
            try{
                DateTime last_event;
                try{
                    last_event = new DateTime(WeatherDatabase.LoadLastWeatherEvent());
                }catch{
                    //we know that we need to make a new one
                    last_event = DateTime.Now;
                    new Thread(CreateNewEvent).Start();
                    WeatherDatabase.SaveLastWeatherEvent(last_event);
                }
                Logger.Log(last_event.Date.ToString());

                //Now we just check to see if we need to create a new one
                DateTime now = DateTime.Now;
                TimeSpan difference = now - last_event;
                TimeSpan week = TimeSpan.FromDays(7);

                DateTime new_event = last_event;
                if(difference == week){
                    new_event = now;
                    new Thread(CreateNewEvent).Start();
                    WeatherDatabase.SaveLastWeatherEvent(new_event);
                }
            }catch(Exception e){
                Logger.Error("Error in main weather thread: " + e.Message);
            }
            

            //We really don't need to do this often, just every few hours or so.
            Thread.Sleep(2 * 60 * 60 * 1000);
        }
    }

    void CreateNewEvent(){
        current_event = new WeatherEvent(DateTime.Now);
        // Logger.Log("Created new Event");

        
        //Now we have to find the channel to send the new weather events in
        Dictionary<ulong, ChannelPermissions> permissions = ChannelDatabase.QuickLoad();
        foreach(KeyValuePair<ulong, ChannelPermissions> pair in permissions){
            if(pair.Value.HasPermission(PermissionType.SEND_WEATHER)){
                //Now we have to find the guild channel
                IReadOnlyDictionary<ulong, DiscordChannel> channels = guild.Channels;
                Logger.Log(channels.Count.ToString());

                foreach(KeyValuePair<ulong, DiscordChannel> channel in channels){
                    if(channel.Key == pair.Key){
                        // Logger.Log("Found Channel");
                        SendWeatherMessage(channel.Value);
                        return;
                    }
                }
            }
        }
    }

    async Task SendWeatherMessage(DiscordChannel channel){
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
            Title = "**Weather of Starswept Tides**",
            Description = "Mrrow!! Here are the weather patterns for this week!"
        };

        string event_description = "It looks like it will ";
        if(current_event.weather_events.Count == 1){
            event_description += $"be {current_event.weather_events[0].Name()} for most of this week!";
        } else {
            event_description += "be a few things this week:";
            foreach(WeatherEventType event_type in current_event.weather_events){
                event_description += "\n" + event_type.Name();
            }
        }
        embed.AddField("__Current Forcast__", event_description);

        string temeprature_description = "Here is the temperature for this week!\n";
        temeprature_description += $"Max Temperature: {current_event.max_temperature_f:F2}ºF ({current_event.max_temperature:F2}ºC)\n";
        temeprature_description += $"Min Temperature: {current_event.min_temperature_f:F2}ºF ({current_event.min_temperature:F2}ºC)\n";
        temeprature_description += $"Average Temperature: {current_event.average_temperature_f:F2}ºF ({current_event.average_temperature:F2}ºC)\n";
        embed.AddField("__This Weeks Temperature__", temeprature_description);

        string misc_description = "";
        misc_description += $"Wind Speed: {current_event.wind_speed/2.0:F2} KPH\n";
        misc_description += $"Cloud Coverage: {current_event.cloud_coverage:F2}%\n";
        misc_description += $"Fog Coverage: {current_event.visibility:F2}%\n";
        embed.AddField("__Misc Weather__", misc_description);
        embed.WithThumbnail(current_event.weather_events[0].GetImageUrl());
        await channel.SendMessageAsync(embed);
    }

    
}

public class WeatherEvent{
    public double average_temperature_f{get{return (average_temperature * 9.0/5.0) + 32.0;}}
    public double average_temperature{get{return (min_temperature + max_temperature)/2.0;}}

    public double min_temperature_f{get{return (min_temperature * 9.0/5.0) + 32.0;}}
    public double max_temperature_f{get{return (max_temperature * 9.0/5.0) + 32.0;}}
    public double min_temperature;
    public double max_temperature;

    public double percepitation;

    public double wind_speed;

    public double visibility;

    public double cloud_coverage;

    public List<WeatherEventType> weather_events;

    public WeatherEvent(DateTime time){
        RandomizeWeatherEvent(time);
        CreateWeatherEvent();
    }

    //Could generate no event
    void CreateWeatherEvent(){
        //Based off of temeprature, we're going to add a rain/snow/hail/storm probability
        double base_thunder_storm_prob = .3;
        double ideal_thunder_storm_temp = (90.0-32.0) * 5.0/9.0;

        weather_events = new List<WeatherEventType>();

        double average_temp = (max_temperature + min_temperature)/2.0;

        double thunderstorm_probability = FindQuadratic(average_temp/100, ideal_thunder_storm_temp/100, .3);

        double base_rain_prob = .8;
        double ideal_cloud_coverage = .6;

        double rain_probability = FindQuadratic(cloud_coverage/100, ideal_cloud_coverage, .9);

        double servere_storm_prob = .15 * rain_probability * thunderstorm_probability;
        
        Random ran = new Random();
        double value = ran.NextDouble();

        if(value < rain_probability){
            if(average_temp < 0){
                weather_events.Add(WeatherEventType.SNOW);
            } else {
                weather_events.Add(WeatherEventType.RAIN);
            }
            if(value < servere_storm_prob){
                weather_events.Add(WeatherEventType.HURRICANE);
            } else
            if(value < thunderstorm_probability){
                weather_events.Add(WeatherEventType.STORM);
            }
        }

        if(weather_events.Count == 0){
            weather_events.Add(WeatherEventType.SUNNY);
        }

        Logger.Log($"Created weather event. Probability value: {value}. Rain chance: {rain_probability}. Severe storm chance: {servere_storm_prob}. Storm chance: {thunderstorm_probability}.");

    }

    double FindQuadratic(double x_vale, double ideal_x, double ideal_y){
        //the final point is always going to be (1,1), so if we roll a 1 it will always be an 100% chance
        //Let's find the function
        //The first function has point (0,0) and apex (ideal_x, ideal_y).
        //We have to put it in a(x-h)^2+k
        double a_one = (ideal_y * -1)/(ideal_x * ideal_x); //Since we want the zero to be (0,0)

        //Now we can do the next quadratic, which is more algebra
        if(ideal_x == 1){
            return a_one * Math.Pow(x_vale - ideal_x, 2) + ideal_y;            // We dont need the extra quadratic anyway
        }
        double a_two = (ideal_y - 1)/ (ideal_x * ideal_x - 2 * ideal_x + 1);

        Logger.Log(a_one + " | " + a_two);

        return (x_vale < ideal_x) ? a_one * Math.Pow(x_vale - ideal_x, 2) + ideal_y : a_two * Math.Pow(x_vale - 1, 2) + 1; 
        
    }

    double CalculateProbability(double current, double ideal, double std_dev, double base_probability){
        double pdf = (1 / (std_dev * Math.Sqrt(2 * Math.PI))) * Math.Exp(-Math.Pow(current - ideal, 2) / (2 * Math.Pow(std_dev, 2)));
        double probability = base_probability * pdf * 100;
        return probability;
    }

    //We have the time here so we can grab temperatures and adjust
    void RandomizeWeatherEvent(DateTime time){
        //First thing to do would be to generate tempratures
        //We need the relative weather data to do this though
        WeatherData data = WeatherDatabase.LoadWeatherData();

        DayWeather? day_to_use = null;
        double average_max = 0;
        double average_min = 0;
        int iter =0;
        double average_wind_speed = 0;
        double min_wind_speed = double.MaxValue;
        double max_wind_speed = double.MinValue;
        foreach(DayWeather day in data.Days){
            if(time.DayOfYear == day.DateTime.DayOfYear){
                day_to_use = day;
                Logger.Log($"Day Variables: Max Temp: {day.TempMax}, Min Temp: {day.TempMin}, Day Temp: {day.Temp}");
            } 
            if(time.Month == day.DateTime.Month){
                iter ++;
                average_max += day.TempMax;
                average_min += day.TempMin;
            }
            average_wind_speed += day.WindSpeed;
            if(day.WindSpeed > max_wind_speed){
                max_wind_speed = day.WindSpeed;
            }
            if(day.WindSpeed < min_wind_speed){
                min_wind_speed = day.WindSpeed;
            }
        }
        average_wind_speed /= data.Days.Count; //We're just grabbing this to randomize our windspeed
        average_max /= iter;
        average_min /= iter;


        if(day_to_use == null){
            throw new Exception("No day data");
        }

        //Okay, now we're just going to choose two random temperatures for the min and max, starting with the min and max of the day
        Random ran = new Random();

        //Min Temp
        double threshold_min = Math.Abs(day_to_use.TempMin - average_min);
        double ran_double = ran.NextDouble();
        double temperature_min = FindPercent(day_to_use.TempMin - threshold_min, day_to_use.TempMin + threshold_min, ran_double);

        //Max Temp
        double threshold_max = Math.Abs(day_to_use.TempMax - average_max);
        ran_double = ran.NextDouble();
        double temperature_max = FindPercent(day_to_use.TempMax - threshold_max, day_to_use.TempMax + threshold_max, ran_double);

        min_temperature = temperature_min;
        max_temperature = temperature_max;

        wind_speed = FindExpodent(min_wind_speed, max_wind_speed, average_wind_speed, ran.NextDouble());

        ran_double = ran.NextDouble();
        visibility = Math.Pow(ran_double, 6) * 100;
        Logger.Log($"Fog coverage: {visibility}. Random: {ran_double}.");

        ran_double = ran.NextDouble();
        cloud_coverage = Math.Pow(ran_double, 1.2) * 100;
        Logger.Log($"Cloud coverage: {cloud_coverage}. Random: {ran_double}.");
    }

    double FindExpodent(double lower, double higher, double average, double percent){
        double scaled_percent = percent - 0.5;

        double expodent = 1 /(1+Math.Exp(scaled_percent));
        return lower + (((higher - lower) + average)/2) * expodent;
    }

    double FindPercent(double lower, double higher, double percentage){
        return lower + ((percentage) * (higher-lower));
    }
}

public enum WeatherEventType{
    RAIN,
    SNOW,
    HAIL,
    STORM,
    HURRICANE,
    SUNNY

}

public static class WeatherEventExtender{
    public static string Name(this WeatherEventType weather){
        switch(weather){
            case WeatherEventType.RAIN :
                return "Rain";
            case WeatherEventType.SNOW :
                return "Snow";
            case WeatherEventType.HAIL :
                return "Hail";
            case WeatherEventType.STORM :
                return "Storm";
            case WeatherEventType.HURRICANE :
                return "Severe Storm";
            case WeatherEventType.SUNNY :
                return "Sunny";
            default:
                return "N/A";
        }
    }

    public static string GetImageUrl(this WeatherEventType weather){
        switch(weather){
            case WeatherEventType.RAIN:
                return "https://media.discordapp.net/attachments/1168749227158016140/1172305978616729701/thumb_720_450_rain-clouds_shutterstock_55994950.jpg?ex=655fd605&is=654d6105&hm=136ad130742e72a7c6018f0c80221846ccdb9a2cae71150537171e2fef5a8f86&=";
            case WeatherEventType.SNOW:
            case WeatherEventType.HAIL:
                return "https://media.discordapp.net/attachments/1168749227158016140/1172306309970919555/npr.brightspotcdn.jpeg?ex=655fd654&is=654d6154&hm=94fd22d95ff5698707c1333307ec4f46af52d650f3b207297c8ef643d038b5ab&=";
            case WeatherEventType.STORM:
            case WeatherEventType.HURRICANE:
                return "https://media.discordapp.net/attachments/1168749227158016140/1172306140219060234/assets.newatlas.webp?ex=655fd62c&is=654d612c&hm=1b7948005f178a9165018aec7149712ca4d65daa3bae73c02031d2ec315b591f&=&width=839&height=559";
            default:
                return "https://cdn.discordapp.com/attachments/1168749227158016140/1172047543782359050/0-1.jpg?ex=655ee556&is=654c7056&hm=dc205c74e2369ccc961a333bbbee0f54329e3bec44176760db7b1b35b16d0b20&";

        }
    }
}

public static class WeatherDatabase{
    static object lock_object = new object();
    const string file_path_weather = "Database/Weather/weather.json";
    const string file_path_weather_data = "Database/Weather/weather_data.json";


    public static long LoadLastWeatherEvent(){
        lock(lock_object){
            if(!File.Exists(file_path_weather)){throw new Exception();}
            string json_data = File.ReadAllText(file_path_weather);
            
            WeatherEventManager.WeatherData data =  JsonConvert.DeserializeObject<WeatherEventManager.WeatherData>(json_data);
            return data.time;
        }
    }

    public static void SaveLastWeatherEvent(DateTime time){
        lock(lock_object){
            File.WriteAllText(file_path_weather, JsonConvert.SerializeObject(new WeatherEventManager.WeatherData(time.Ticks)));
        }
    }

    public static WeatherData LoadWeatherData(){
        lock(lock_object){
            string json_data = File.ReadAllText(file_path_weather_data);
            WeatherData data = JsonConvert.DeserializeObject<WeatherData>(json_data);
            return data;
        }
    }

}

//Objects for database extraction
public class WeatherData{
    [JsonProperty("latitude")]
    public double Latitude { get; set; }

    [JsonProperty("longitude")]
    public double Longitude { get; set; }

    [JsonProperty("resolvedAddress")]
    public string ResolvedAddress { get; set; }

    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("timezone")]
    public string Timezone { get; set; }

    [JsonProperty("tzoffset")]
    public double TimezoneOffset { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("days")]
    public List<DayWeather> Days { get; set; }

    [JsonConstructor]
    public WeatherData(
        double latitude, double longitude, string resolvedAddress, string address,
        string timezone, double tzoffset, string name, List<DayWeather> days){
        Latitude = latitude;
        Longitude = longitude;
        ResolvedAddress = resolvedAddress;
        Address = address;
        Timezone = timezone;
        TimezoneOffset = tzoffset;
        Name = name;
        Days = days;
    }
}


public class DayWeather{
    [JsonProperty("datetime")]
    public DateTime DateTime { get; set; }

    [JsonProperty("datetimeEpoch")]
    public long DateTimeEpoch { get; set; }

    [JsonProperty("tempmax")]
    public double TempMax { get; set; }

    [JsonProperty("tempmin")]
    public double TempMin { get; set; }

    [JsonProperty("temp")]
    public double Temp { get; set; }

    [JsonProperty("feelslikemax")]
    public double FeelsLikeMax { get; set; }

    [JsonProperty("feelslikemin")]
    public double FeelsLikeMin { get; set; }

    [JsonProperty("feelslike")]
    public double FeelsLike { get; set; }

    [JsonProperty("dew")]
    public double Dew { get; set; }

    [JsonProperty("humidity")]
    public double Humidity { get; set; }

    [JsonProperty("precip")]
    public double Precipitation { get; set; }

    [JsonProperty("precipprob")]
    public double PrecipitationProbability { get; set; }

    [JsonProperty("precipcover")]
    public double PrecipitationCover { get; set; }

    [JsonProperty("preciptype")]
    public List<string> PrecipitationType { get; set; }

    [JsonProperty("snow")]
    public double Snow { get; set; }

    [JsonProperty("snowdepth")]
    public double SnowDepth { get; set; }

    [JsonProperty("windgust")]
    public double WindGust { get; set; }

    [JsonProperty("windspeed")]
    public double WindSpeed { get; set; }

    [JsonProperty("winddir")]
    public double WindDirection { get; set; }

    [JsonProperty("pressure")]
    public double Pressure { get; set; }

    [JsonProperty("cloudcover")]
    public double CloudCover { get; set; }

    [JsonProperty("visibility")]
    public double Visibility { get; set; }

    [JsonProperty("solarradiation")]
    public double SolarRadiation { get; set; }

    [JsonProperty("solarenergy")]
    public double SolarEnergy { get; set; }

    [JsonProperty("uvindex")]
    public double UVIndex { get; set; }

    [JsonProperty("severerisk")]
    public double SevereRisk { get; set; }

    [JsonProperty("sunrise")]
    public string Sunrise { get; set; }

    [JsonProperty("sunriseEpoch")]
    public long SunriseEpoch { get; set; }

    [JsonProperty("sunset")]
    public string Sunset { get; set; }

    [JsonProperty("sunsetEpoch")]
    public long SunsetEpoch { get; set; }

    [JsonProperty("moonphase")]
    public double MoonPhase { get; set; }

    [JsonProperty("conditions")]
    public string Conditions { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("icon")]
    public string Icon { get; set; }

    [JsonProperty("stations")]
    public List<string> Stations { get; set; }

    [JsonProperty("source")]
    public string Source { get; set; }

    [JsonConstructor]
    public DayWeather(
        DateTime dateTime, long dateTimeEpoch, double tempMax, double tempMin, double temp,
        double feelsLikeMax, double feelsLikeMin, double feelsLike, double dew, double humidity,
        double precipitation, double precipitationProbability, double precipitationCover,
        List<string> precipitationType, double snow, double snowDepth, double windGust,
        double windSpeed, double windDirection, double pressure, double cloudCover, double visibility,
        double solarRadiation, double solarEnergy, double uvIndex, double severeRisk, string sunrise,
        long sunriseEpoch, string sunset, long sunsetEpoch, double moonPhase, string conditions,
        string description, string icon, List<string> stations, string source){
        DateTime = dateTime;
        DateTimeEpoch = dateTimeEpoch;
        TempMax = tempMax;
        TempMin = tempMin;
        Temp = temp;
        FeelsLikeMax = feelsLikeMax;
        FeelsLikeMin = feelsLikeMin;
        FeelsLike = feelsLike;
        Dew = dew;
        Humidity = humidity;
        Precipitation = precipitation;
        PrecipitationProbability = precipitationProbability;
        PrecipitationCover = precipitationCover;
        PrecipitationType = precipitationType;
        Snow = snow;
        SnowDepth = snowDepth;
        WindGust = windGust;
        WindSpeed = windSpeed;
        WindDirection = windDirection;
        Pressure = pressure;
        CloudCover = cloudCover;
        Visibility = visibility;
        SolarRadiation = solarRadiation;
        SolarEnergy = solarEnergy;
        UVIndex = uvIndex;
        SevereRisk = severeRisk;
        Sunrise = sunrise;
        SunriseEpoch = sunriseEpoch;
        Sunset = sunset;
        SunsetEpoch = sunsetEpoch;
        MoonPhase = moonPhase;
        Conditions = conditions;
        Description = description;
        Icon = icon;
        Stations = stations;
        Source = source;
    }
}