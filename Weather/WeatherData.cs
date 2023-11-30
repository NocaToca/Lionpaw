

using Newtonsoft.Json;

namespace Lionpaw{

    namespace Weather{

        
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
    }

}