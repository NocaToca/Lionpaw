using Newtonsoft.Json;
using Lionpaw.Channels;
using Lionpaw.Commands;
using DSharpPlus.Entities;
using Lionpaw.Databases;
using Lionpaw.Queries;
using Lionpaw.Subscriptions;
using Lionpaw.Messages;

namespace Lionpaw{

    [JsonObject]
    public class Guild{

        //Related to permissions
        private struct CommandOptions{
            [JsonProperty("enabled")]
            public bool enabled = true;

            [JsonConstructor]
            public CommandOptions(bool enabled){
                this.enabled = enabled;
            }
        }

        private struct DatabaseCommandOptions{
            [JsonProperty("character_name")]
            public string character_name = "character"; //Appended to add/edit/remove

            [JsonConstructor]
            public DatabaseCommandOptions(string character_name){
                this.character_name = character_name;
            }   
        }

        [JsonProperty("id")]
        public ulong id;

        [JsonProperty("channels")]
        public List<Channel> channels = new List<Channel>();


        [JsonProperty("database_options")]
        private DatabaseCommandOptions database_options;

        [JsonProperty("command_options")]
        private Dictionary<Command, CommandOptions> options;

        [JsonProperty("queries")]
        private List<Query> queries;

        [JsonProperty("subscriptions")]
        private List<Subscription> subscriptions;

        [JsonProperty("messages")]
        public Dictionary<MessageType, Message> messages;

        
        #pragma warning disable 8618
        [JsonConstructor]
        private Guild(ulong id, List<Channel> channels, DatabaseCommandOptions database_command_options, Dictionary<Command, CommandOptions> options, List<Query> queries, List<Subscription> subscriptions,
        Dictionary<MessageType, Message> messages){
            this.id = id;
            this.channels = channels;
            this.database_options = database_command_options;
            this.options = options;
            this.queries = queries;
            this.subscriptions = subscriptions;
            this.messages = messages;
        }

        //Used when registering a new guild
        public Guild(ulong id, List<Channel> channels){
            this.id = id;
            this.channels = channels;

            database_options = new DatabaseCommandOptions(); //This will already give the default options
            SetDefaultOptions(this);

            queries = new List<Query>();

            SaveGuild();

        }

        public Guild(DiscordGuild guild){
            id = guild.Id;

            List<Channel> channels = new List<Channel>();
            foreach(var key in guild.Channels){
                channels.Add(new Channel(key.Key));
            }

            this.channels = channels;

            database_options = new DatabaseCommandOptions();
            SetDefaultOptions(this);

            queries = new List<Query>();

            SaveGuild();
        }
        #pragma warning restore

        public async Task SetupSubscribers(Lionpaw bot){
            foreach(Subscription subscription in this.subscriptions){
                await subscription.SubscribeToEvent(bot);
            }
        }

        private DatabaseCommandModule database_commands;
        private GuildCommandModule guild_commands;
        private ShowCommandsModule show_commands;
        private SubscriptionModule subscription_module;
        private SillyModule silly_module;
        private StatisticsModule statistics_module;
        private VisualModule visual_module;
        public void CreateCommands(out DatabaseCommandModule database_commands, out GuildCommandModule guild_commands,
        out ShowCommandsModule show_commands, out SubscriptionModule subscription_module, out SillyModule silly_module,
        out StatisticsModule statistics_module, out VisualModule visual_module){
            database_commands = new DatabaseCommandModule(this);
            this.database_commands = database_commands;

            guild_commands = new GuildCommandModule(this);
            this.guild_commands = guild_commands;

            show_commands = new ShowCommandsModule(this);
            this.show_commands = show_commands;

            subscription_module = new SubscriptionModule(this);
            this.subscription_module = subscription_module;

            silly_module = new SillyModule(this);
            this.silly_module = silly_module;

            statistics_module = new StatisticsModule(this);
            this.statistics_module = statistics_module;

            visual_module = new VisualModule(this);
            this.visual_module = visual_module;
        }

        //Done to make the function inline
        private Action<Guild> SetDefaultOptions = (g) => {
            g.options = new Dictionary<Command, CommandOptions>{
                //Database Module
                { Command.DATA_ADD, new CommandOptions()},
                { Command.REMOVE, new CommandOptions() },
                { Command.EDIT, new CommandOptions() },
                { Command.INCREMENT, new CommandOptions() },

                //Display Module
                { Command.SHOW_CHARACTER, new CommandOptions() },
                { Command.SHOW_ALL, new CommandOptions() },
                { Command.SHOW_FILTERED, new CommandOptions(){enabled = false} },
                { Command.SHOW_USER, new CommandOptions() },

                //Guild Module is the only global module

                //SillyModule
                {Command.EightBall, new CommandOptions()}
            };
        };

        public void SaveGuild(){
            DatabaseAccessor.Save<Guild>(this, DatabaseAccessor.GUILD_SAVE_PATH(id));
        }

        public void AddQuery(Query query){
            Logger.Log("New Parameter added for guild", id);
            queries.Add(query);
            SaveGuild();
        }

        public void RemoveQuery(string parameter){
            for(int i = 0; i < queries.Count; i++){
                if(queries[i].name == parameter){
                    queries.RemoveAt(i);
                    SaveGuild();
                    return;
                }
            }
        }

        //We want a copy, not a reference.
        public List<Query> GetQueries(){
            List<Query> queries = new List<Query>();
            foreach(Query query in this.queries){
                queries.Add(query);
            }
            return queries;
        }

        public void ToggleModule(Module module, bool enabling){

        }

        public void ToggleCommand(Command command, bool enabling){

        }

        public async Task AddSubscription(Subscription subscription){
            subscriptions.Add(subscription);
            await subscription.CreateSubscriptionEvent();
            SaveGuild();
            await Lionpaw.MainBot.RefreshCommands();
        }

        public async Task RemoveSubscription(string description){
            foreach(Subscription subscription in subscriptions){
                if(subscription.Description() == description){
                    subscriptions.Remove(subscription);
                    break;
                }
            }
            SaveGuild();
            await Lionpaw.MainBot.RefreshCommands();
        }

        public IReadOnlyList<Subscription> GetSubscriptions(){
            return (IReadOnlyList<Subscription>)subscriptions;
        }

        public bool AddMessage(MessageType type, Message message){
            if(messages.ContainsKey(type)){
                return false;
            }
            messages.Add(type, message);
            return true;
        }
        public bool EditMessage(MessageType type, Message message){
            try{
                messages[type] = message;
                return true;
            }catch{
                return false;
            }
        }

    }

}