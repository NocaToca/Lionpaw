using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Lionpaw;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Entities;

//All of our events that our bot runs
public partial class Bot{

    private async Task Goodbye(DiscordClient sender, GuildMemberRemoveEventArgs args){
        //First let's load our permissions
        Dictionary<ulong, ChannelPermissions> permissions = ChannelDatabase.QuickLoad();

        //Then grab our channels
        IReadOnlyDictionary<ulong, DiscordChannel> guild_channels = args.Guild.Channels;

        ulong goodbye_channel_id = 0;
        foreach(KeyValuePair<ulong, ChannelPermissions> perm_pair in permissions){
            if(perm_pair.Value.HasPermission(PermissionType.GOODBYE)){
                goodbye_channel_id = perm_pair.Key;
                break;
            }
        }

        try{
            DiscordChannel goodbye = guild_channels[goodbye_channel_id];

            string user_name = args.Member.DisplayName;

            await goodbye.SendMessageAsync($"{user_name} has left the server. May the stars grant them safe travels.");

        }finally{
        }
    }

    private async Task Welcome(DiscordClient sender, GuildMemberAddEventArgs args){

        //First let's load our permissions
        Dictionary<ulong, ChannelPermissions> permissions = ChannelDatabase.QuickLoad();

        //Then grab our channels
        IReadOnlyDictionary<ulong, DiscordChannel> guild_channels = args.Guild.Channels;

        ulong welcome_channel_id = 0;
        foreach(KeyValuePair<ulong, ChannelPermissions> perm_pair in permissions){
            if(perm_pair.Value.HasPermission(PermissionType.WELCOME)){
                welcome_channel_id = perm_pair.Key;
                break;
            }
        }

        try{
            DiscordChannel welcome = guild_channels[welcome_channel_id];
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**✧⋄⋆⋅⋆⋄Welcome to Starwept Tides⋄⋆⋅⋆⋄✧**",
            };

            string user_mention = args.Member.Mention;
            List<Lionpaw.Channel> channels = ChannelDatabase.LoadChannels();

            string rules_mention = "rules";
            string introduction_mention = "introductions";
            foreach(Lionpaw.Channel channel in channels){
                if(channel.channel_type == Lionpaw.ChannelType.RULES){
                    rules_mention = guild_channels[channel.channel_id].Mention;
                } else 
                if(channel.channel_type == Lionpaw.ChannelType.INTRODUCTION){
                    introduction_mention = guild_channels[channel.channel_id].Mention;
                }
            }

            //Now we can build our description
            string description = "**Hello! Mrrow!**\n\n";
            description += $"Welcome to Starswept Tides {user_mention}! Before you jump in, we as you to look over our {rules_mention} and make a quick {introduction_mention} so we can get to know you better! ";
            description += "After that, you're free to hang out with us in a chatting channel, or get right to making your character!";
            description += "\n\nBe sure to hit show all channels in the server settings so channels don't disappear on you! Mrreow!";

            embed.WithDescription(description);

            await welcome.SendMessageAsync(embed.Build());

        }finally{
        }

    }

    private Task ClientError(DiscordClient sender, ClientErrorEventArgs args){
        Logger.Log($"Error in event handler: {args.Exception.Message} found in {args.EventName}.");
        return Task.CompletedTask;
    }

    

    private Task OnReady(DiscordClient sender, ReadyEventArgs e){

        Console.WriteLine("Bot Ready!");


        return Task.CompletedTask;

    }

    private async Task OnButtonPress(DiscordClient sender, ComponentInteractionCreateEventArgs e){
        if(e.Interaction.Data.CustomId == "create_submissions_channel"){
            DiscordChannel channel = DiscordUtils.CreatePrivateChannel(sender, await e.Guild.GetMemberAsync(e.User.Id), e.Guild).Result;
            await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Created your private channel at " + channel.Mention));
        } else
        if(e.Interaction.Data.CustomId == "delete_submissions_channel"){
            await DiscordUtils.DeleteChannel(e.Channel);
        }

        
    }

}