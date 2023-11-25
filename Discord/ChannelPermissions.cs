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

//This is a bit different than Dsharp.DiscordPermissions
//We want to give Lionpaw special Permissions that aren't related to moderating - well not entirely
//Want Lionpaw to respond to commands in certain channels? Here's where you set that up!

namespace Lionpaw{

    //Stores and writes saved permissions
    public static partial class ChannelDatabase{

        internal class PermissionGrab{
            [JsonProperty("type")]
            internal ChannelType type;

            [JsonProperty("permissions")]
            internal ChannelPermissions permissions;

            [JsonConstructor]
            public PermissionGrab(ChannelType type, ChannelPermissions permissions){
                this.type = type;
                this.permissions = permissions;
            }
        }


        public static ChannelPermissions GetChannelPermissionSettings(ChannelType type){
            string file_path = "Database/channels_permissions.json";

            // Check if the file exists
            if (File.Exists(file_path)){
                // If the file exists, load the existing data
                string json_data = File.ReadAllText(file_path);
                List<PermissionGrab>? channels = JsonConvert.DeserializeObject<List<PermissionGrab>>(json_data);

                if(channels == null){
                    //Return default permissions
                    return new ChannelPermissions(ChannelPermissions.DEFAULT);
                }

                foreach(PermissionGrab grab in channels){
                    if(grab.type == type){
                        return grab.permissions;
                    }
                }
            }

            return new ChannelPermissions(ChannelPermissions.DEFAULT);
        }

        public static void SaveChannelPermissionSettings(ChannelType type, ChannelPermissions permissions){
            // Define the file path
            string file_path = "Database/channels_permissions.json";

            // Console.WriteLine(type);
            // Console.WriteLine(permissions.permission);

            PermissionGrab grab = new PermissionGrab(type, permissions);

            // Create a list to hold all channels
            List<PermissionGrab>? channels = new List<PermissionGrab>();

            // Check if the file exists
            if (File.Exists(file_path)){
                // If the file exists, load the existing data
                string json_data = File.ReadAllText(file_path);
                channels = JsonConvert.DeserializeObject<List<PermissionGrab>>(json_data);
                if(channels == null){
                    channels = new List<PermissionGrab>();
                }

                PermissionGrab? existing_channel = channels.FirstOrDefault(c => c.type == type);
                if(existing_channel != null){

                    List<PermissionGrab> new_list = new List<PermissionGrab>();
                    foreach(PermissionGrab perms in channels){
                        if(perms.type != type){
                            new_list.Add(perms);
                        } else {
                            new_list.Add(grab);
                        }
                    }

                    // Serialize the list back to JSON
                    string _updated_json_data = JsonConvert.SerializeObject(new_list);

                    // Write the updated JSON data back to the file
                    File.WriteAllText(file_path, _updated_json_data);

                    return;
                }
            }

            // Add the new channel to the list
            channels.Add(grab);
            // Console.WriteLine(channels.Count);S

            // Serialize the list back to JSON
            string updated_json_data = JsonConvert.SerializeObject(channels);

            // Write the updated JSON data back to the file
            File.WriteAllText(file_path, updated_json_data);
        }
    }

    public enum PermissionType{
        [ChoiceName("Read Messages")]
        READ_MESSAGES,
        [ChoiceName("Show Commands")]
        SHOW_COMMANDS,
        [ChoiceName("Respond")]
        RESPOND,
        [ChoiceName("Welcome")]
        WELCOME,
        [ChoiceName("Goodbye")]
        GOODBYE,
        [ChoiceName("Send Weather Events")]
        SEND_WEATHER
    }

    [JsonObject]
    public struct ChannelPermissions{

        [JsonProperty("permission_integer")]
        public ulong permission; 
        //This can be represented by a power of 2
        //Essentially we just bitwise OR permissions

        //Here's a list of all of our permissions
        public const ulong DEFAULT = 0; //No permissions = default
        public const ulong READ_MESSAGES = 1 << ((int)PermissionType.READ_MESSAGES);
        public const ulong WELCOME = 1 << ((int)PermissionType.WELCOME);
        public const ulong GOODBYE = 1 << ((int)PermissionType.GOODBYE);
        public const ulong RESPOND = 1 << ((int)PermissionType.RESPOND);
        public const ulong SHOW_COMMANDS = 1 << ((int)PermissionType.SHOW_COMMANDS);
        public const ulong SEND_WEATHER = 1 << ((int)PermissionType.SEND_WEATHER);

        [JsonConstructor]
        public ChannelPermissions(ulong permission){
            this.permission = permission;
        }

        public ChannelPermissions(PermissionType type){
            this.permission = (ulong)(1 << (int)type);
        }

        public void AddPermission(PermissionType type){
            #pragma warning disable 0675
            permission = (permission | (ulong)(1 << ((int)type)));
            #pragma warning restore
        }

        public bool HasPermission(PermissionType type){
            return (permission & (ulong)(1 << (int)type)) != 0;
        }

        public void RemovePermission(PermissionType type){
            permission = permission & ~(ulong)(1 << (int)type);
        }

        public List<PermissionType> GetPermissions(){
            List<PermissionType> types = new List<PermissionType>();

            try{
                for(int i = 0; i < 64; i++){
                    if(HasPermission((PermissionType)i)){
                        types.Add((PermissionType)i);
                    }
                }
            }catch(Exception e){
                Logger.Error(e.Message);
            }
            return types;
        }
    }

}


