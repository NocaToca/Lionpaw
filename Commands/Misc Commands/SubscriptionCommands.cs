using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lionpaw.Subscriptions;

namespace Lionpaw{


    namespace Commands{

        public class SubscriptionModule : Module{

            public SubscriptionCommands commands;

            public SubscriptionModule(Guild guild) : base(guild){
                commands = new SubscriptionCommands(this);
            }

            [SlashCommandGroup("subscribe", "Subscription commands.")]
            public class SubscriptionCommands : ApplicationCommandModule{

                public SubscriptionModule module;
                public AddCommands add_commands;
                public RemoveCommands remove_commands;

                public SubscriptionCommands(SubscriptionModule module){
                    this.module = module;
                    add_commands = new AddCommands(module);
                    remove_commands = new RemoveCommands(module);
                }
                #pragma warning disable 8618
                public SubscriptionCommands(){

                }
                #pragma warning restore

                [SlashCommandGroup("add", "Handles all adding functions for subscription commands")]
                public class AddCommands : ApplicationCommandModule{

                    public SubscriptionModule module;

                    public AddCommands(SubscriptionModule module){
                        this.module = module;
                    }
                    #pragma warning disable 8618
                    public AddCommands(){}

                    [SlashCommand("role", "Adds a role after the fired event")]
                    public static async Task AddRole(InteractionContext ctx,
                    [Option("role","The discord role to add.")] DiscordRole role,
                    [Option("event", "The event to subscribe to.")] OnEvent event_trigger,
                    [ChoiceProvider(typeof(Lionpaw.QueryOptionChoiceProvider))]
                    [Option("conditional", "Require the token conditional to be met for the event to fire.")]string conditional_token = ""){
                        await Lionpaw.MainBot.CallCommandForGuild<SubscriptionCommands.AddCommands>(ctx)._AddRole(role, event_trigger, conditional_token);
                    }

                    private async Task _AddRole(DiscordRole role, OnEvent event_trigger, string conditional_token){
                        Subscription subscription = new Subscription(new Role(role.Id), SubscriptionType.ROLE_ADD, event_trigger, null);

                        if(conditional_token != ""){
                            subscription.conditional = new Conditional(conditional_token);
                        }

                        await module.parent.AddSubscription(subscription);
                    }

                }

                [SlashCommandGroup("remove", "Handles all removing functions for subscription commands")]
                public class RemoveCommands : ApplicationCommandModule{

                    public SubscriptionModule module;
                    public RemoveCommands(SubscriptionModule module){
                        this.module = module;
                    }
                    public RemoveCommands(){}

                    [SlashCommand("role", "Removes a role after the fired event")]
                    public static async Task RemoveRole(InteractionContext ctx,
                    [ChoiceProvider(typeof(Lionpaw.SubscriptionChoiceProviderRole))]
                    [Option("subscription", "The subscription to remove")] string subscription_description){
                        await Lionpaw.MainBot.CallCommandForGuild<SubscriptionCommands.RemoveCommands>(ctx)._RemoveRole(subscription_description);
                    }

                    private async Task _RemoveRole(string subscription_description){
                        
                        await module.parent.RemoveSubscription(subscription_description);
                    }
                }
            }

            

        }
        
    }

}