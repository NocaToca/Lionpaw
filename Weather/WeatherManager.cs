
using DSharpPlus.Entities;
using Lionpaw.Channels;
using Lionpaw.Databases;
using Newtonsoft.Json;

namespace Lionpaw{

    namespace Weather{

        public class WeatherEventManager{

            public struct WeatherSaveData{
                [JsonProperty("time")]
                public long time;

                [JsonProperty("enabled")]
                public bool enabled;

                [JsonConstructor]
                public WeatherSaveData(long time, bool enabled){
                    this.time = time;
                    this.enabled = enabled;
                }

                public DateTime ToDateTime(){
                    return new DateTime(time);
                }
            }

            WeatherEvent current_event;

            Guild guild;

            public WeatherEventManager(Guild guild){
                this.guild = guild;
            }

            //Our main weatherevents thread
            public async Task StartWeather(){
                while(true){
                    // Logger.Log("Creating Event");
                    try{
                        WeatherSaveData last_event;
                        try{
                            last_event = await DatabaseAccessor.LoadItem<WeatherSaveData>(DatabaseAccessor.WEATHER_SAVE_DATA_PATH(guild.id));
                        }catch{
                            //we know that we need to make a new one
                            last_event = new WeatherSaveData(0, false);
                            await DatabaseAccessor.Save<WeatherSaveData>(last_event, DatabaseAccessor.WEATHER_SAVE_DATA_PATH(guild.id));
                        }

                        //Now we just check to see if we need to create a new one
                        if(!last_event.enabled){
                            return;
                        }

                        DateTime now = DateTime.Now;
                        TimeSpan difference = now - last_event.ToDateTime();
                        TimeSpan week = TimeSpan.FromDays(7);

                        WeatherSaveData new_event = last_event;
                        if(difference >= week){
                            new_event.time = now.Ticks;
                            await CreateNewEvent();
                            await DatabaseAccessor.Save<WeatherSaveData>(new_event, DatabaseAccessor.WEATHER_SAVE_DATA_PATH(guild.id));

                        }
                    }catch(Exception e){
                        await Logger.Error("Error in main weather thread: " + e.Message);
                    }
                    

                    //We really don't need to do this often, just every few hours or so.
                    await Task.Delay(60 * 60 * 1000 * 2);
                }
            }

            async Task CreateNewEvent(){
                current_event = new WeatherEvent(DateTime.Now, guild);
                // Logger.Log("Created new Event");

                
                //Now we have to find the channel to send the new weather events in
                List<Channel> channels = guild.channels;
                foreach(Channel channel in channels){
                    if(channel.permissions.HasPermission(ChannelSetting.SEND_WEATHER)){
                        //Now we have to find the guild channel
                        DiscordGuild discord_guild =  await Lionpaw.MainBot.GetGuild(guild);
                        IReadOnlyDictionary<ulong, DiscordChannel> discord_channels = discord_guild.Channels;

                        foreach(KeyValuePair<ulong, DiscordChannel> discord_channel in discord_channels){
                            if(channel.channel_id == discord_channel.Key){
                                // Logger.Log("Found Channel");
                                await SendWeatherMessage(discord_channel.Value);
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

            Guild guild;

            public WeatherEvent(DateTime time, Guild guild){
                this.guild = guild;
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

                return (x_vale < ideal_x) ? a_one * Math.Pow(x_vale - ideal_x, 2) + ideal_y : a_two * Math.Pow(x_vale - 1, 2) + 1; 
                
            }

            double CalculateProbability(double current, double ideal, double std_dev, double base_probability){
                double pdf = (1 / (std_dev * Math.Sqrt(2 * Math.PI))) * Math.Exp(-Math.Pow(current - ideal, 2) / (2 * Math.Pow(std_dev, 2)));
                double probability = base_probability * pdf * 100;
                return probability;
            }

            //We have the time here so we can grab temperatures and adjust
            async Task RandomizeWeatherEvent(DateTime time){
                //First thing to do would be to generate tempratures
                //We need the relative weather data to do this though
                WeatherData data = await DatabaseAccessor.LoadItem<WeatherData>(DatabaseAccessor.WEATHER_DATA_PATH(guild.id));

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

                ran_double = ran.NextDouble();
                cloud_coverage = Math.Pow(ran_double, 1.2) * 100;
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

    }

}