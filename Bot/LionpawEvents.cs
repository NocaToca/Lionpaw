using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DiscordExtensions;
using Lionpaw.Channels;
using Lionpaw.Databases;
using Lionpaw.Statistics;
using Lionpaw.Messages;

namespace Lionpaw{


    public partial class Lionpaw
    {

        private async Task Goodbye(DiscordClient sender, GuildMemberRemoveEventArgs args){
            //First let's load our permissions
            try{    
                Guild guild = registered_guilds[args.Guild.Id];

                foreach(Channel channel in guild.channels){
                    if(channel.permissions.HasPermission(ChannelSetting.WELCOME)){
                        string content = guild.messages[Messages.MessageType.GOODBYE].GetContent();

                        DiscordEmbedBuilder embed = Messages.MessageType.GOODBYE.CreateEmbed(args.Guild.Name);
                        embed.WithDescription(content);

                        await args.Guild.GetChannel(channel.channel_id).SendMessageAsync(embed);
                    }
                }
            }catch{

            }

        }

        private async Task Welcome(DiscordClient sender, GuildMemberAddEventArgs args){

            //First let's load our permissions

            try{    
                Guild guild = registered_guilds[args.Guild.Id];

                foreach(Channel channel in guild.channels){
                    if(channel.permissions.HasPermission(ChannelSetting.WELCOME)){
                        string content = guild.messages[Messages.MessageType.WELCOME].GetContent();

                        DiscordEmbedBuilder embed = Messages.MessageType.WELCOME.CreateEmbed(args.Guild.Name);
                        embed.WithDescription(content);

                        await args.Guild.GetChannel(channel.channel_id).SendMessageAsync(embed);
                    }
                }
            }catch{

            }


        }

        private async Task ClientError(DiscordClient sender, ClientErrorEventArgs args){
            await Logger.Error(args.Exception.Message);
        }



        private Task OnReady(DiscordClient sender, ReadyEventArgs e){

            Console.WriteLine("Bot Ready!");
            Console.WriteLine("Registered Guilds: " + registered_guilds.Count);

            return Task.CompletedTask;
        }

        private async Task OnButtonPress(DiscordClient sender, ComponentInteractionCreateEventArgs e){
            if (e.Interaction.Data.CustomId == "create_submissions_channel"){
                DiscordChannel channel = DiscordUtils.CreatePrivateChannel(sender, await e.Guild.GetMemberAsync(e.User.Id), e.Guild).Result;
                await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().AsEphemeral().WithContent("Created your private channel at " + channel.Mention));
            }
            else
            if (e.Interaction.Data.CustomId == "delete_submissions_channel"){
                await DiscordUtils.DeleteChannel(e.Channel);
            }
        }

        private async Task OnMessageRecieved(DiscordClient sender, MessageCreateEventArgs args){
            string content = args.Message.Content;
            try{
                Guild guild = registered_guilds[args.Guild.Id];

                if(await StatisticsAccessor.IsUser(args.Author.Id)){
                    foreach(Channel channel in guild.channels){
                        if(channel.channel_id == args.Channel.Id){
                            if(channel.permissions.HasPermission(ChannelSetting.READ_ROLEPLAYS)){
                                await RoleplayMessageParser.StartParsing(content, args.Author.Id, args.Guild.Id);
                                return;
                            } else {
                                return;
                            }
                        }
                    }
                }

            }catch{


            }
            
        }

        

    }
    
}