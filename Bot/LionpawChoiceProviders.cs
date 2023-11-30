
using System.Runtime.InteropServices;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lionpaw.Characters;
using Lionpaw.Databases;
using Lionpaw.Queries;
using Lionpaw.Subscriptions;


namespace Lionpaw{

    public partial class Lionpaw{

        public abstract class CustomChoiceProvider : ChoiceProvider{

            public static List<DiscordApplicationCommandOptionChoice> choices = new List<DiscordApplicationCommandOptionChoice>();

            public static object lock_object = new object();

            public abstract void RefreshDatabase();

            public override Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider(){
                lock(lock_object){
                    RefreshDatabase();

                    return Task.FromResult((IEnumerable<DiscordApplicationCommandOptionChoice>)choices);
                }
            }
        }

        //Used by Show Filtered
        public class QueryOptionChoiceProvider : CustomChoiceProvider{

            public override void RefreshDatabase(){
                choices.Clear();

                if(GuildId == null){
                    return;
                }

                foreach(Query query in MainBot.registered_guilds[(ulong)GuildId].GetQueries()){
                    foreach(string token in query.tokens){
                        choices.Add(new DiscordApplicationCommandOptionChoice(query.name +": " + token, token));

                    }
                }
            }

            

        }

        public class QueryChoiceProvider : CustomChoiceProvider{


            public override void RefreshDatabase(){
                choices.Clear();

                if(GuildId == null){
                    return;
                }

                foreach(Query query in MainBot.registered_guilds[(ulong)GuildId].GetQueries()){
                    choices.Add(new DiscordApplicationCommandOptionChoice(query.name, query.name));
                }
            } 
        }

        public class CharacterChoiceProvider : CustomChoiceProvider{
            public override async void RefreshDatabase(){
                choices.Clear();

                if(GuildId == null){
                    return;
                }

                List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH((ulong)GuildId));

                foreach(Character character in characters){
                    choices.Add(new DiscordApplicationCommandOptionChoice(character.name, character.name));
                }
            } 

        }

        public class SubscriptionChoiceProviderRole : CustomChoiceProvider{

            public override void RefreshDatabase(){
                choices.Clear();

                if(GuildId == null){
                    return;
                }

                IReadOnlyList<Subscription> subscriptions = MainBot.registered_guilds[(ulong)GuildId].GetSubscriptions();

                foreach(Subscription subscription in subscriptions){
                    if(subscription.subscription_type == SubscriptionType.ROLE_ADD || subscription.subscription_type == SubscriptionType.ROLE_REMOVE){
                        choices.Add(new DiscordApplicationCommandOptionChoice(subscription.Description(), subscription.Description()));
                    }
                }
            }

        }

    }

}