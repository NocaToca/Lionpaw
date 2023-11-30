using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace Lionpaw{

    namespace Channels{

        //Channel representaion
        public class Channel{

            [JsonProperty("channel_id")]
            public ulong channel_id;

            //Permissions
            [JsonProperty("channel_permissions")]
            public ChannelPermissions permissions;

            //Base constructor

            [JsonConstructor]
            public Channel(ulong id, ChannelPermissions permissions){
                channel_id = id;
                this.permissions = permissions;
            }

            public Channel(ulong id){
                channel_id = id;
                permissions = new ChannelPermissions(ChannelPermissions.DEFAULT);
            }

            public string GetDetail(DiscordGuild containing_guild){
                string name = containing_guild.GetChannel(channel_id).Name;
                return name + " | " + permissions.ToString();
            }

            
        }


        [JsonObject]
        public struct ChannelPermissions{

            [JsonProperty("permission_integer")]
            public ulong permission; 
            //This can be represented by a power of 2
            //Essentially we just bitwise OR permissions

            //Here's a list of all of our permissions
            public const ulong DEFAULT = 0; //No permissions = default
            public const ulong READ_MESSAGES = 1 << ((int)ChannelSetting.READ_MESSAGES);
            public const ulong WELCOME = 1 << ((int)ChannelSetting.WELCOME);
            public const ulong RESPOND = 1 << ((int)ChannelSetting.RESPOND);
            public const ulong SHOW_COMMANDS = 1 << ((int)ChannelSetting.SHOW_COMMANDS);
            public const ulong SEND_WEATHER = 1 << ((int)ChannelSetting.SEND_WEATHER);
            public const ulong READ_ROLEPLAYS = 1 << ((int)ChannelSetting.READ_ROLEPLAYS);

            [JsonConstructor]
            public ChannelPermissions(ulong permission){
                this.permission = permission;
            }

            public ChannelPermissions(ChannelSetting type){
                this.permission = (ulong)(1 << (int)type);
            }

            public void AddPermission(ChannelSetting type){
                #pragma warning disable 0675
                permission = (permission | (ulong)(1 << ((int)type)));
                #pragma warning restore
            }

            public bool HasPermission(ChannelSetting type){
                return (permission & (ulong)(1 << (int)type)) != 0;
            }

            public void RemovePermission(ChannelSetting type){
                permission = permission & ~(ulong)(1 << (int)type);
            }

            public List<ChannelSetting> GetPermissions(){
                List<ChannelSetting> types = new List<ChannelSetting>();

                try{
                    for(int i = 0; i < 64; i++){
                        if(HasPermission((ChannelSetting)i)){
                            types.Add((ChannelSetting)i);
                        }
                    }
                }catch(Exception e){
                    Logger.Error(e.Message);
                }
                return types;
            }
        }

        public enum ChannelSetting{
            [ChoiceName("Read Messages")]
            READ_MESSAGES,
            [ChoiceName("Show Commands")]
            SHOW_COMMANDS,
            [ChoiceName("Respond")]
            RESPOND,
            [ChoiceName("Welcome")]
            WELCOME,
            [ChoiceName("Send Weather Events")]
            SEND_WEATHER,
            [ChoiceName("Read Roleplay Messages")]
            READ_ROLEPLAYS
        }

    }

}