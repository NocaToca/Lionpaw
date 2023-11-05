//This class is to help Lionpaw understand the purpose for channels and react to them accordingly
//He stores information about these channels internally
using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using DSharpPlus.SlashCommands;


namespace Lionpaw{
    public enum ChannelType{
        [ChoiceName("Welcome")]
        WELCOME,
        [ChoiceName("Goodbye")]
        LEAVING,
        [ChoiceName("Roleplay")]
        ROLEPLAY,
        [ChoiceName("General")]
        GENERAL,
        [ChoiceName("Plotting")]
        PLOTTING,
        [ChoiceName("Staff")]
        STAFF,
        [ChoiceName("Bot Test")]
        BOT_TEST,
        [ChoiceName("Bot Dump")]
        BOT_SPAM,
        [ChoiceName("Submissions Channel")]
        SUBMISSION,
        //Shouldn't ever be set manually, Lionpaw will do this himself
        SUBMISSION_TEMP,
        [ChoiceName("Partner")]
        PARTNER,
        [ChoiceName("Lore")]
        LORE,
        [ChoiceName("Rules")]
        RULES,
        [ChoiceName("Announcements")]
        ANNOUNCEMENTS,
        [ChoiceName("Introductions")]
        INTRODUCTION
    }

    //Stores and writes channels
    public static partial class ChannelDatabase{

        public static void SaveChannels(List<Channel> channels){
            foreach(Channel channel in channels){
                SaveChannel(channel);
            }
        }

        //Conflicts are detected by channel id
        public static void SaveChannel(Channel channel){
            // Define the file path
            string file_path = "Database/channels.json";

            // Create a list to hold all channels
            List<Channel>? channels = new List<Channel>();

            // Check if the file exists
            if (File.Exists(file_path)){
                // If the file exists, load the existing data
                string json_data = File.ReadAllText(file_path);
                channels = JsonConvert.DeserializeObject<List<Channel>>(json_data);
                if(channels == null){
                    channels = new List<Channel>();
                }

                Channel? existing_channel = channels.FirstOrDefault(c => c.channel_id == channel.channel_id);
                if(existing_channel != null){
                    List<Channel> new_channels = new List<Channel>();
                    foreach(Channel chan in channels){
                        if(chan.channel_id == channel.channel_id){
                            new_channels.Add(channel);
                        } else {
                            new_channels.Add(chan);
                        }
                    }

                    // Serialize the list back to JSON
                    string _updated_json_data = JsonConvert.SerializeObject(new_channels);

                    // Write the updated JSON data back to the file
                    File.WriteAllText(file_path, _updated_json_data);

                    return;
                }
            }

            // Add the new channel to the list
            channels.Add(channel);

            // Serialize the list back to JSON
            string updated_json_data = JsonConvert.SerializeObject(channels);

            // Write the updated JSON data back to the file
            File.WriteAllText(file_path, updated_json_data);
        }

        //Auto converts to a dictionary for us
        public static Dictionary<ulong, ChannelPermissions> QuickLoad(){
            return CreateReadableDictionary(LoadChannels());
        }

        public static Dictionary<ulong, Channel> LoadChannelsAsDictionary(){
            List<Channel> channels = LoadChannels();

            Dictionary<ulong, Channel> channel_dictionary = new Dictionary<ulong, Channel>();

            foreach(Channel channel in channels){
                channel_dictionary.Add(channel.channel_id, channel);
            }

            return channel_dictionary;
        }

        public static Dictionary<ulong, ChannelPermissions> CreateReadableDictionary(List<Channel> channels){
            Dictionary<ulong, ChannelPermissions> permissions_dictionary = new Dictionary<ulong, ChannelPermissions>();
            foreach(Channel channel in channels){
                permissions_dictionary.Add(channel.channel_id, channel.permissions);
            } 

            return permissions_dictionary;
        }

        public static List<Channel> LoadChannels(){
            string file_path = "Database/channels.json";

            // Check if the file exists
            if (File.Exists(file_path)){
                // If the file exists, load the existing data
                string json_data = File.ReadAllText(file_path);
                List<Channel>? channels = JsonConvert.DeserializeObject<List<Channel>>(json_data);

                if(channels == null){
                    throw new Exception("No cats to load");
                }

                return channels;
            }

            throw new Exception("No cats to load");
        }

    }

    

    //Channel representaion
    public class Channel{
        [JsonProperty("channel_type")]
        public ChannelType channel_type;

        [JsonProperty("channel_id")]
        public ulong channel_id;

        //Permissions
        [JsonProperty("channel_permissions")]
        public ChannelPermissions permissions;

        //Base constructor
        public Channel(ChannelType type, ulong id){
            this.channel_type = type;
            this.channel_id = id;
            permissions = new ChannelPermissions(ChannelDatabase.GetChannelPermissionSettings(type).permission);
        }

        [JsonConstructor]
        public Channel(ChannelType type, ulong id, ChannelPermissions permissions){
            channel_type = type;
            channel_id = id;
            this.permissions = permissions;
        }

        public string GetDetail(DiscordGuild containing_guild){
            string name = containing_guild.GetChannel(channel_id).Name;
            return name + " | " + channel_type.ToString();
        }

        
    }
}
