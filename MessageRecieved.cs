using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using DSharpPlus.SlashCommands;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Lionpaw;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;

//We do quick a lot in this method, so I wanted to save it in a seperate file
public partial class Bot{

    private async Task OnMessageRecieved(DiscordClient sender, MessageCreateEventArgs args){
        string content = args.Message.Content;
        Dictionary<ulong, ChannelPermissions> permissions = ChannelDatabase.QuickLoad();

        try{
            if(permissions[args.Channel.Id].HasPermission(PermissionType.RESPOND)){
                if(Regex.Match(content, @"lionpaw", RegexOptions.IgnoreCase).Length > 0){
                    System.Random random = new System.Random();
                    int ran = random.Next(0, Cat.cat_speak.Length);
                    await args.Message.RespondAsync(Cat.cat_speak[ran]);
                }
            }

            //We're going to handle roleplay replies for stats here
            if(permissions[args.Channel.Id].HasPermission(PermissionType.READ_MESSAGES)){
                //There's a lot we have to go through lol
                Dictionary<ulong, Lionpaw.Channel> channels = ChannelDatabase.LoadChannelsAsDictionary();

                //Check if it's a roleplay channel
                if(channels[args.Channel.Id].channel_type == Lionpaw.ChannelType.ROLEPLAY){
                    //We now have to check to see if we can look at this user (just if their directory exists)
                    if(Roleplay.RoleplayDatabase.IsUser(args.Author.Id)){
                        //Now we can handle things appropriately 
                        Roleplay.RoleplayMessageParser.StartParsing(args.Message.Content, args.Author.Id);
                    }
                }

            }

        }catch(Exception e){}finally{
        }
    }

}