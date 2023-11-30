using System.ServiceModel.Channels;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lionpaw.Channels;
using Lionpaw.Databases;
using Lionpaw.Messages;
using Lionpaw.Queries;

namespace Lionpaw{


    namespace Commands{

        public class GuildCommandModule : Module{
            public ModuleCommands module_commands;
            public CommandCommands command_commands;
            public ParameterCommands parameter_commands;
            public SettingsCommands settings_commands;


            public GuildCommandModule(Guild guild) : base(guild){
                module_commands = new ModuleCommands(this);
                command_commands = new CommandCommands(this);
                parameter_commands = new ParameterCommands(this);
                settings_commands = new SettingsCommands(this);
            }

            //This command is global
            public class GuildRegisterCommand : ApplicationCommandModule{
                [SlashCommand("register", "Registers your guild with Lionpaw!")]
                public static async Task Register(InteractionContext ctx){
                    if(ctx.Channel.GuildId == null){
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This command is not enabled in dms!"));
                    }

                    //Let's make sure it's not already registered
                    if(!DatabaseAccessor.DirectoryExist(DatabaseAccessor.GUILD_PATH(ctx.Guild.Id)).Result){
                        Guild guild = new Guild(ctx.Guild);
                        
                        await GuildAccessor.RegisterGuild(guild);
                        await Lionpaw.MainBot.RefreshGuilds();

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Guild registered!"));

                    } else {
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Guild already registered!"));
                    }
                }
            }

            //These commands are not
            [SlashCommandGroup("module", "Related to command modules")]
            public class ModuleCommands : ApplicationCommandModule{
                public Module module;
                #pragma warning disable 8618
                public ModuleCommands(){

                }
                #pragma warning restore

                public ModuleCommands(GuildCommandModule commands){
                    module = commands;
                }

                [SlashCommand("enable", "Enables all commands within the module")]
                public static async Task EnableModule(InteractionContext ctx,
                [Option("module", "Module to enable")] Module module){
                    await Lionpaw.MainBot.CallCommandForGuild<ModuleCommands>(ctx).Toggle(module, true);
                }

                public Task Toggle(Module module, bool enabling){
                    module.parent.ToggleModule(module, enabling);
                    return Task.CompletedTask;
                }

                [SlashCommand("disable", "Disables all commands within the module")]
                public static async Task DisableModule(InteractionContext ctx,
                [Option("module", "Module to disalbe")] Module module){
                    await Lionpaw.MainBot.CallCommandForGuild<ModuleCommands>(ctx).Toggle(module, false);

                }

            }

            [SlashCommandGroup("command", "Related to basic commands")]
            public class CommandCommands : ApplicationCommandModule{
                public Module module;

                #pragma warning disable 8618
                public CommandCommands(){

                }
                #pragma warning restore
                public CommandCommands(GuildCommandModule commands){
                    module = commands;
                }

                [SlashCommand("enable", "Enables all commands within the module")]
                public static async Task EnableCommand(InteractionContext ctx,
                [Option("module", "Module to enable")] Command command){
                    await Lionpaw.MainBot.CallCommandForGuild<CommandCommands>(ctx).Toggle(command, true);
                }

                public Task Toggle(Command command, bool enabling){
                    module.parent.ToggleCommand(command, enabling);
                    return Task.CompletedTask;
                }

                [SlashCommand("disable", "Disables all commands within the module")]
                public static async Task DisableCommand(InteractionContext ctx,
                [Option("module", "Module to disalbe")] Command command){
                    await Lionpaw.MainBot.CallCommandForGuild<CommandCommands>(ctx).Toggle(command, false);

                }



            }

            [SlashCommandGroup("parameter", "Related to parameters.")]
            public class ParameterCommands : ApplicationCommandModule{
                public Module module;

                //This needs to be here so discord doesnt crash
                #pragma warning disable 8618
                public ParameterCommands(){
                }
                #pragma warning restore
                public ParameterCommands(GuildCommandModule commands){
                    module = commands;
                }

                [SlashCommand("add", "Adds a parameter for Lionpaw to query")]
                public static async Task AddParameter(InteractionContext ctx,
                [Option("name", "The name of your custom parameter")] string name ="",
                [Option("description", "The description that you want the parameter to have.")] string description = "",
                [Option("tokens", "The list of tokens that Lionpaw will relate this parameter to (seperate by commas).")] string tokens = "",
                [Option("type", "The type of value this parameter will be")] QueryValue  value = QueryValue.NUMBER,
                [Option("default", "A list of default options")]QueryType query_type = QueryType.CUSTOM){
                    
                    Query query;


                    //We'll want to do all of this work only if our query isn't custom
                    if(query_type == QueryType.CUSTOM){
                        query = new Query(name, description, tokens.Replace(" ", "").Split(',').ToList(), query_type, value);
                    } else {
                        query = new Query(query_type);
                    }   


                    await Lionpaw.MainBot.CallCommandForGuild<ParameterCommands>(ctx).AddParameterFunction(query);

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Parameter {query.name} Added!").AsEphemeral());

                    await Lionpaw.MainBot.RefreshCommands();
                }

                private async Task AddParameterFunction(Query query){
                    try{
                        module.parent.AddQuery(query);
                    }catch(Exception e){
                        await Logger.Error(e.Message);
                    }
                }

                [SlashCommand("remove", "Removes a parameter for Lionpaw to query")]
                public static async Task RemoveParameter(InteractionContext ctx,
                [ChoiceProvider(typeof(Lionpaw.QueryChoiceProvider))]
                [Option("parameter", "The parameter to remove")]string parameter){
                    await Lionpaw.MainBot.CallCommandForGuild<ParameterCommands>(ctx).RemoveParameterFunction(parameter);

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Parameter {parameter} Removed!").AsEphemeral()); 

                    await Lionpaw.MainBot.RefreshCommands();
                }

                public Task RemoveParameterFunction(string parameter){
                    module.parent.RemoveQuery(parameter);
                    return Task.CompletedTask;
                }

                [SlashCommand("edit", "Edits an existing parameter for Lionpaw to query.")]
                public static async Task EditParameter(InteractionContext ctx,
                [ChoiceProvider(typeof(Lionpaw.QueryChoiceProvider))]
                [Option("parameter", "The parameter to edit")] string parameter,
                [Option("name", "The name of your custom parameter")] string name ="",
                [Option("description", "The description that you want the parameter to have.")] string description = "",
                [Option("tokens", "The list of tokens that Lionpaw will relate this parameter to (seperate by commas).")] string tokens = "",
                [Option("type", "The type of value this parameter will be")] QueryValue  value = QueryValue.NUMBER,
                [Option("default", "A list of default options")]QueryType query_type = QueryType.CUSTOM){

                    Query query;

                    if(query_type == QueryType.CUSTOM){
                        query = new Query(name, description, tokens.Replace(" ", "").Split(',').ToList(), query_type, value);
                    } else {
                        query = new Query(query_type);
                    }   

                    ParameterCommands commands = Lionpaw.MainBot.CallCommandForGuild<ParameterCommands>(ctx);

                    //We don't need to make an extra function here
                    await commands.RemoveParameterFunction(parameter);
                    await commands.AddParameterFunction(query);

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Edited {parameter}!").AsEphemeral()); 

                    await Lionpaw.MainBot.RefreshCommands();
                }

            }

            [SlashCommandGroup("settings", "Related to changing guild settings")]
            public class SettingsCommands : ApplicationCommandModule{
                public Module module;
                #pragma warning disable 8618
                public SettingsCommands(){

                }
                #pragma warning restore

                public LionpawSettings lionpaw_settings;

                public ChannelSettings channel_settings;

                public SettingsCommands(GuildCommandModule commands){
                    module = commands;
                    lionpaw_settings = new LionpawSettings(this);
                    channel_settings = new ChannelSettings(this);
                }

                // [SlashCommand("command", "The settings for a giving command")]
                // public static async Task EditCommandSettings(InteractionContext ctx){} //To be implemented

                [SlashCommandGroup("lionpaw", "Settings relating to the display of Lionpaw")]
                public class LionpawSettings : ApplicationCommandModule{

                    public SettingsCommands parent;
                    #pragma warning disable 8618
                    public LionpawSettings(){

                    }
                    #pragma warning restore

                    public LionpawSettings(SettingsCommands commands){
                        parent = commands;
                    }

                    [SlashCommand("nick", "Change Lionpaw's nick")]
                    public static async Task ChangeNick(InteractionContext ctx,
                    [Option("name", "Lionpaw's new nick")]string nick){
                        DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id); //The bot

                        await member.ModifyAsync(member => member.Nickname = nick);
                    }

                    [SlashCommand("pfp", "Change Lionpaw's pfp")]
                    public static async Task ChangePFP(InteractionContext ctx,
                    [Option("pfp", "Lionpaw's new pfp")]DiscordAttachment pfp){
                        DiscordMember member = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id); //The bot
                    }
                }

                [SlashCommandGroup("channel", "Settings related to managing guild channels")]
                public class ChannelSettings : ApplicationCommandModule{

                    public SettingsCommands parent;
                    #pragma warning disable 8618
                    public ChannelSettings(){}
                    #pragma warning restore

                    public ChannelSettings(SettingsCommands commands){
                        parent = commands;
                    }

                    [SlashCommand("permit", "Adds a permission to a channel/category")]
                    public static async Task Permit(InteractionContext ctx, 
                    [Option("channel", "The discord channel or category to edit")]DiscordChannel channel,
                    [Option("setting", "The setting to permit.")] ChannelSetting setting){
                        await Lionpaw.MainBot.CallCommandForGuild<ChannelSettings>(ctx).Edit(ctx, channel, setting, AddPermission);

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Updated Channel!").AsEphemeral());
                    }

                    public async Task Edit(InteractionContext ctx, DiscordChannel channel, ChannelSetting settings, Action<DiscordChannel, Guild, ChannelSetting> edit_action){
                        Guild guild = Lionpaw.MainBot.registered_guilds[ctx.Guild.Id];
                        if(channel.IsCategory){
                            foreach(DiscordChannel child_channel in channel.Children){
                                await Task.Run(() => {edit_action(child_channel, guild, settings);});
                            }
                        } else {
                            await Task.Run(()=>{edit_action(channel, guild, settings);});
                        }

                        guild.SaveGuild();
                    }

                    public static void AddPermission(DiscordChannel discord_channel, Guild guild, ChannelSetting settings){

                        foreach(Channel channel in guild.channels){
                            if(channel.channel_id == discord_channel.Id){
                                channel.permissions.AddPermission(settings);
                            }
                        }

                    }

                    [SlashCommand("revoke", "Removes a permission to a channel/category")]
                    public static async Task Revoke(InteractionContext ctx, 
                    [Option("channel", "The discord channel or category to edit")]DiscordChannel channel,
                    [Option("setting", "The setting to permit.")] ChannelSetting setting){
                        await Lionpaw.MainBot.CallCommandForGuild<ChannelSettings>(ctx).Edit(ctx, channel, setting, RevokePermission);

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Updated Channel!").AsEphemeral());
                    }

                    public static void RevokePermission(DiscordChannel discord_channel, Guild guild, ChannelSetting settings){

                        foreach(Channel channel in guild.channels){
                            if(channel.channel_id == discord_channel.Id){
                                channel.permissions.AddPermission(settings);
                            }
                        }

                    }

                }

                [SlashCommandGroup("message", "Sets or creates a message of a supported Lionpaw function.")]
                public class MessageCommands : ApplicationCommandModule{

                    public SettingsCommands parent;

                    public MessageCommands(SettingsCommands commands){
                        parent = commands;
                    }
                    #pragma warning disable 8618
                    public MessageCommands(){}
                    #pragma warning restore

                    [SlashCommand("set", "Sets a new message type. Type in your message or link.")]
                    public static async Task AddMessage(InteractionContext ctx,
                    [Option("type", "The type of message to add.")]MessageType type,
                    [Option("message", "The message to display.")]string message){

                        await Lionpaw.MainBot.CallCommandForGuild<MessageCommands>(ctx)._AddMessage(ctx, type, message);
                    }

                    public async Task _AddMessage(InteractionContext ctx, MessageType type, string message){
                        Guild guild = parent.module.parent;

                        Messages.Message message_class = new Messages.Message(message);
                        guild.AddMessage(type, new Messages.Message(message));

                        DiscordEmbedBuilder embed = type.CreateEmbed(ctx.Channel.Guild.Name);
                        embed.WithDescription(message_class.GetContent());

                        await ctx.CreateResponseAsync(embed);
                    }


                }

            }

        }
        
    }

}