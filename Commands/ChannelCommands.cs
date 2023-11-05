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
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace Lionpaw{

    //Essentially these will just handle channel set up for us
    public class ChannelCommands{

        public static List<DiscordChannel> GetChannelsInCategory(string name, DiscordGuild guild){
            IReadOnlyDictionary<ulong, DiscordChannel> channels = guild.Channels;
            List<DiscordChannel> discord_channels = new List<DiscordChannel>();

            //Lets iterate through and see if the belong to the right category
                foreach(KeyValuePair<ulong, DiscordChannel> pair in channels){
                    if(pair.Value.IsCategory){
                        if(Regex.Match(pair.Value.Name.ToString(), name, RegexOptions.IgnoreCase).Length > 0){
                            //This is the right category
                            discord_channels.AddRange(pair.Value.Children);
                            break;
                        }
                    }
                }
            return discord_channels;
        }

        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashCommandGroup("register", "Registers Channels")]
        public class RegisterCommands : ApplicationCommandModule{
            
            [SlashCommand("category", "Registers all channels within the category regarding the type provided")]
            public async Task RegisterCategory(InteractionContext ctx, 
            [Option("Name", "The (partial) name of the category.")] string name,
            [Option("Type", "The type of the channels that are in this category")] ChannelType type){
                
                List<DiscordChannel> discord_channels = GetChannelsInCategory(name, ctx.Channel.Guild);

                List<Channel> saving_channels = new List<Channel>();
                foreach(DiscordChannel channel in discord_channels){
                    saving_channels.Add(new Channel(type, channel.Id));
                }

                ChannelDatabase.SaveChannels(saving_channels);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Successfully added {saving_channels.Count} channels!"));
            }

            [SlashCommand("channel", "Registers the channel regarding to information provided")]
            public async Task RegisterChannel(InteractionContext ctx, 
            [Option("channel", "The discord channel to add")] DiscordChannel channel,
            [Option("Type", "The type of the channel that this is")] ChannelType type){

                Channel channel_ = new Channel(type, channel.Id);

                ChannelDatabase.SaveChannel(channel_);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Saved channel!"));
            }
        }

        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashCommandGroup("display", "Display Channels")]
        public class DisplayCommands : ApplicationCommandModule{
            [SlashCommand("all", "Show Lionpaw's internal channel database")]
            public async Task ShowAll(InteractionContext ctx,
            [Option("page", "The page.")] long page){
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                    Title = "Channels"
                };

                List<Channel> channels;
                try{
                    channels = ChannelDatabase.LoadChannels();
                }catch(Exception e){
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Problem grabbing from database: " + e.Message));
                    return;
                }

                int channelsPerPage = 20;
                int startIndex = ((int)page - 1) * channelsPerPage;
                int endIndex = Math.Min(startIndex + channelsPerPage, channels.Count);

                string description = "";
                for(int i = startIndex; i < endIndex; i++){
                    description += "\n" + channels[i].GetDetail(ctx.Channel.Guild);
                }

                embed.Description = description;

                embed.WithFooter($"{page}/{Math.Ceiling(channels.Count/20.0)}");

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
            }
            

            [SlashCommand("channel", "Displays the current permission information about the given channel")]
            public async Task ShowChannel(InteractionContext ctx,
            [Option("channel", "The channel to show information about")] DiscordChannel channel){
                Dictionary<ulong, ChannelPermissions> dictionary = ChannelDatabase.QuickLoad();

                string description = "";
                try{
                    List<PermissionType> get_permissions = dictionary[channel.Id].GetPermissions();
                    
                    foreach(PermissionType type in get_permissions){
                        description += type.ToString() + "\n";
                    }
                } catch (Exception e){
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You need to register the channel before viewing!"));
                }

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                    Title = "**Channel Permissions**",
                };

                embed.AddField("__PERMISSIONS__", description);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
            }
        }

        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashCommandGroup("permission", "Sets the default permissions of a category or channel")]
        public class PermissionCommands: ApplicationCommandModule{

            [SlashCommandGroup("add", "Adds permissions to the category, channel, or ChannelType")]
            public class AddCommands : ApplicationCommandModule{

                [SlashCommand("category", "Adds the permission to the specified category")]
                public async Task AddToCategory(InteractionContext ctx,
                [Option("name", "The name of the Category.")] string name,
                [Option("permission", "The type of permission to add.")] PermissionType permission_type){
                    List<DiscordChannel> discord_channels = GetChannelsInCategory(name, ctx.Channel.Guild);

                    List<Channel> old_channels = ChannelDatabase.LoadChannels();

                    List<Channel> updated_permissions = new List<Channel>();
                    foreach(DiscordChannel channel in discord_channels){
                        foreach(Channel channel_rep in old_channels){
                            if(channel.Id == channel_rep.channel_id){
                                channel_rep.permissions.AddPermission(permission_type);
                                updated_permissions.Add(channel_rep);
                            }
                        }
                    }

                    ChannelDatabase.SaveChannels(updated_permissions);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Updated Permissions to {updated_permissions.Count} channels!"));
                }

                [SlashCommand("channel", "Adds the specified permission to the channel")]
                public async Task AddToChannel(InteractionContext ctx,
                [Option("channel", "The Channel.")] DiscordChannel channel,
                [Option("permission", "The type of permission to add.")] PermissionType permission_type){

                    List<Channel> old_channels = ChannelDatabase.LoadChannels();

                    List<Channel> updated_permissions = new List<Channel>();
                    foreach(Channel channel_rep in old_channels){
                        if(channel.Id == channel_rep.channel_id){
                            channel_rep.permissions.AddPermission(permission_type);
                            Console.WriteLine(channel_rep.permissions.permission);
                            updated_permissions.Add(new Channel(channel_rep.channel_type, channel.Id, channel_rep.permissions));
                        }
                    }

                    ChannelDatabase.SaveChannels(updated_permissions);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Updated Permissions to {updated_permissions.Count} channel!"));
                }

                [SlashCommand("type", "Adds the default permissions for the channel type")]
                public async Task AddToType(InteractionContext ctx,
                [Option("channel", "The Channel.")] ChannelType type,
                [Option("permission", "The type of permission to add.")] PermissionType permission_type){

                    ChannelPermissions permissions = ChannelDatabase.GetChannelPermissionSettings(type);
                    permissions.AddPermission(permission_type);
                    ChannelDatabase.SaveChannelPermissionSettings(type, permissions);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Updated Permissions to {type} channel!"));
                }

            }

            [SlashCommandGroup("remove", "Removes permissions to the category, channel, or ChannelType")]
            public class RemoveCommands : ApplicationCommandModule{

                [SlashCommand("category", "Removes the permission from the specified category")]
                public async Task AddToCategory(InteractionContext ctx,
                [Option("name", "The name of the Category.")] string name,
                [Option("permission", "The type of permission to add.")] PermissionType permission_type){
                    List<DiscordChannel> discord_channels = GetChannelsInCategory(name, ctx.Channel.Guild);

                    List<Channel> old_channels = ChannelDatabase.LoadChannels();

                    List<Channel> updated_permissions = new List<Channel>();
                    foreach(DiscordChannel channel in discord_channels){
                        foreach(Channel channel_rep in old_channels){
                            if(channel.Id == channel_rep.channel_id){
                                channel_rep.permissions.RemovePermission(permission_type);
                                updated_permissions.Add(channel_rep);
                            }
                        }
                    }

                    ChannelDatabase.SaveChannels(updated_permissions);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Updated Permissions to {updated_permissions.Count} channels!"));
                }

                [SlashCommand("channel", "Removes the specified permission from the channel")]
                public async Task AddToChannel(InteractionContext ctx,
                [Option("channel", "The Channel.")] DiscordChannel channel,
                [Option("permission", "The type of permission to add.")] PermissionType permission_type){

                    List<Channel> old_channels = ChannelDatabase.LoadChannels();

                    List<Channel> updated_permissions = new List<Channel>();
                        foreach(Channel channel_rep in old_channels){
                            if(channel.Id == channel_rep.channel_id){
                                channel_rep.permissions.RemovePermission(permission_type);
                                updated_permissions.Add(channel_rep);
                            }
                        }

                    ChannelDatabase.SaveChannels(updated_permissions);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Updated Permissions to {updated_permissions.Count} channel!"));
                }

                [SlashCommand("type", "Removes the default permissions for the channel type")]
                public async Task AddToType(InteractionContext ctx,
                [Option("channel", "The Channel.")] ChannelType type,
                [Option("permission", "The type of permission to add.")] PermissionType permission_type){

                    ChannelPermissions permissions = ChannelDatabase.GetChannelPermissionSettings(type);
                    permissions.RemovePermission(permission_type);
                    ChannelDatabase.SaveChannelPermissionSettings(type, permissions);

                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Updated Permissions to {type} channel!"));
                }

            }

        }

        
            
    }

}