using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lionpaw.Commands;
using Lionpaw.Databases;

namespace Lionpaw{
    
    public partial class Lionpaw{

        //We need this to kind of fake guild specific commands
        public Dictionary<ulong, Guild> registered_guilds;
        public Dictionary<Guild, List<ApplicationCommandModule>> guild_commands;

        //MainBot is set on startup
        #pragma warning disable 8618
        public static Lionpaw MainBot;
        #pragma warning restore

        private async Task Setup(){

            MainBot = this;

            //Register Guild specific commands
            await RefreshGuilds();

            //Load Global Commands
            commands.RegisterCommands<GuildCommandModule.GuildRegisterCommand>(1174584271164428308);
            commands.RegisterCommands<GuildCommandModule.GuildRegisterCommand>(1178863849613557770);

            //Load Listeners

            //Set Up Listeners
            await SubscribeListeners();
        }

        public async Task RefreshGuilds(){

            await Logger.Log("Refreshed", 0);

            registered_guilds = new Dictionary<ulong, Guild>();
            guild_commands = new Dictionary<Guild, List<ApplicationCommandModule>>();

            Guild[] guilds = await GuildAccessor.LoadAllGuilds();


            foreach(Guild guild in guilds){
                DatabaseCommandModule database_commands;
                GuildCommandModule guild_command_module;
                ShowCommandsModule show_commands_module;
                SillyModule sill_module;
                SubscriptionModule subscription_module;
                StatisticsModule statistics_module;
                VisualModule visual_module;

                guild.CreateCommands(out database_commands, out guild_command_module, out show_commands_module, out subscription_module, out sill_module, out statistics_module, out visual_module);

                await Logger.Log("Registering Commands: " + guild.id, 0);

                await guild.SetupSubscribers(this);

                //Database module
                commands.RegisterCommands(typeof(DatabaseCommandModule.AddCommand), guild.id);

                //Guild Module
                commands.RegisterCommands<GuildCommandModule.ParameterCommands>(guild.id);
                commands.RegisterCommands<GuildCommandModule.CommandCommands>(guild.id);
                commands.RegisterCommands<GuildCommandModule.ModuleCommands>(guild.id);
                commands.RegisterCommands<GuildCommandModule.SettingsCommands.LionpawSettings>(guild.id);
                commands.RegisterCommands<GuildCommandModule.SettingsCommands.ChannelSettings>(guild.id);

                //Show Module
                commands.RegisterCommands<ShowCommandsModule.ShowCommands>(guild.id);

                //Silly Module
                commands.RegisterCommands<SillyModule.SillyCommands>(guild.id);
                
                //Subscription Commands
                commands.RegisterCommands<SubscriptionModule.SubscriptionCommands.AddCommands>(guild.id);
                commands.RegisterCommands<SubscriptionModule.SubscriptionCommands.RemoveCommands>(guild.id);

                //Statistics Module
                commands.RegisterCommands<StatisticsModule.StatisticsCommands>(guild.id);

                //Visual Module
                commands.RegisterCommands<VisualModule.ChartCommands>(guild.id);

                registered_guilds.Add(guild.id, guild);
                guild_commands.Add(guild, new List<ApplicationCommandModule>(){
                    //Database Module
                    database_commands.add_commands,

                    //Guild Module
                    guild_command_module.parameter_commands,
                    guild_command_module.command_commands,
                    guild_command_module.module_commands,
                    guild_command_module.settings_commands.lionpaw_settings,
                    guild_command_module.settings_commands.channel_settings,

                    //Show Module
                    show_commands_module.show_command,

                    //Silly Module
                    sill_module.silly_commands,

                    //Subscription Commands
                    subscription_module.commands.add_commands,
                    subscription_module.commands.remove_commands,

                    //Statistics Module
                    statistics_module.statistics_commands,

                    //Visual Module
                    visual_module.chart_commands,

                });

            }

        }

        public async Task<DiscordGuild> GetGuild(Guild guild){
            if(client == null){
                throw new Exception();
            }
            return await client.GetGuildAsync(guild.id);
        }

        public async Task RefreshCommands(){
            await commands.RefreshCommands();
        }

        private async Task SubscribeListeners(){
            if(client == null){
                throw new Exception();
            }
            
            //Submissions button response
            await Task.Run(() =>{
                client.ComponentInteractionCreated += OnButtonPress;
                client.MessageCreated += OnMessageRecieved;
                client.GuildMemberAdded += Welcome;
                client.GuildMemberRemoved += Goodbye;
                client.ClientErrored += ClientError;
            });
        }

        public T CallCommandForGuild<T>(InteractionContext ctx) where T : ApplicationCommandModule{
            try{
                foreach(ApplicationCommandModule command in guild_commands[registered_guilds[ctx.Guild.Id]]){
                    if(command is T casted_command){
                        return casted_command;
                    }
                }
            }catch(Exception e){
                Logger.Error("Command for guild not found: " + e.Message);

            }
            

            throw new Exception();
        }

    }

}