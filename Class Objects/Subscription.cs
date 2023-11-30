
using DSharpPlus.Entities;
using Lionpaw.Characters;
using Newtonsoft.Json;

namespace Lionpaw{

    namespace Subscriptions{


        //Represents a single subscription event
        public class Subscription{

            [JsonProperty("parameter")]
            public SubscriptionParameter parameter;

            [JsonProperty("type")]
            public SubscriptionType subscription_type;

            [JsonProperty("event")]
            public OnEvent triggered_event;

            [JsonProperty("conditional")]
            public Conditional? conditional;

            [JsonConstructor]
            public Subscription(SubscriptionParameter parameter, SubscriptionType type, OnEvent triggered_event, Conditional? conditional){
                this.parameter = parameter;
                this.subscription_type = type;
                this.triggered_event = triggered_event;
                this.conditional = conditional;
            }

            //Now we have to generate the subscription event
            public async Task<Func<object?, DiscordGuild, Lionpaw, Task>> CreateSubscriptionEvent(){

                //Here we'll define base events
                async Task GrantRole(Character character, SubscriptionParameter parameter, DiscordGuild guild){
                    DiscordMember member = await guild.GetMemberAsync(character.user_id);
                                    
                    if(parameter is Role role){    
                        await member.GrantRoleAsync(guild.GetRole(role.role_id));
                    } else {
                        return;
                    }
                }

                async Task RevokeRole(Character character, SubscriptionParameter parameter, DiscordGuild guild){
                    DiscordMember member = await guild.GetMemberAsync(character.user_id);
                                    
                    if(parameter is Role role){    
                        await member.RevokeRoleAsync(guild.GetRole(role.role_id));
                    } else {
                        return;
                    }
                }

                //We'll need to do different things depending on our subscription type
                switch(subscription_type){

                    case SubscriptionType.ROLE_ADD:
                        return await RoleEvent(GrantRole);
                    case SubscriptionType.ROLE_REMOVE:
                        return await RoleEvent(RevokeRole);
                    default:
                        throw new Exception();
                }
            }

            #pragma warning disable 1998
            public async Task<Func<object?, DiscordGuild, Lionpaw, Task>> RoleEvent(Func<Character, SubscriptionParameter, DiscordGuild, Task> role_event){
                //We will need to know what called this event
                switch(triggered_event){
                    case OnEvent.CHARACTER_ADDED:
                    case OnEvent.CHARACTER_REMOVED:
                        return async (o, guild, bot) => {
                            if(o == null){
                                throw new Exception();
                            }

                            Character character = (Character)o;
                            if(character == null){
                                return;
                            }

                            //We need to check if we have a conditional
                            if(conditional != null){
                                //Conditionals are used for token matching
                                if(character.HasToken(conditional.token)){
                                    await role_event(character, parameter, guild);
                                }
                            } else {
                                await role_event(character, parameter, guild);
                            }
                        };
                    default:
                        throw new Exception();
                }
            }

            public async Task SubscribeToEvent(Lionpaw bot){
                switch(triggered_event){
                    case OnEvent.CHARACTER_ADDED:
                        bot.OnCharacterAdded += new Lionpaw.CustomEvent(CreateSubscriptionEvent().Result);
                        return;
                    case OnEvent.CHARACTER_REMOVED:
                        bot.OnCharacterRemoved += new Lionpaw.CustomEvent(CreateSubscriptionEvent().Result);
                        return;
                    default:
                        return;
                }
            }
            #pragma warning restore


            public string OnEventDescription(){
                switch(triggered_event){

                    case OnEvent.CHARACTER_ADDED:
                        return "Char_Add";
                    case OnEvent.CHARACTER_REMOVED:
                        return "Char_Rem";
                    default:
                        return "N/A";
                }
            }

            public string ActionDescription(){
                switch(subscription_type){
                    case SubscriptionType.ROLE_ADD:
                        return "Role_Add";
                    case SubscriptionType.ROLE_REMOVE:
                        return "Role_Rem";
                    default:
                        return "N/A";
                }
            }

            public string GetConditionalDescription(){
                if(conditional != null){
                    return "if " + conditional.token;
                } else {
                    return "";
                }
            }

            //Builds a brief description of the event
            public string Description(){
                return OnEventDescription() +"|" + GetConditionalDescription();
            }
        }

        //Parent class for subscription parameters like roles
        public abstract class SubscriptionParameter{
            public abstract string Describe();
        }

        //Storable class for discord roles
        [JsonObject]
        public class Role : SubscriptionParameter{

            [JsonProperty("role_id")]
            public ulong role_id;

            public Role(DiscordRole role){
                role_id = role.Id;
            }

            [JsonConstructor]
            public Role(ulong role_id){
                this.role_id = role_id;
            }

            public override string Describe(){
                throw new NotImplementedException();
            }

        }

        //A conditional event for subscriptions
        [JsonObject]
        public class Conditional{
            [JsonProperty("token")]
            public string token;

            [JsonConstructor]
            public Conditional(string token){
                this.token = token;
            }
        }


        public enum SubscriptionType{

            ROLE_ADD,
            ROLE_REMOVE,

        }

        public enum OnEvent{

            CHARACTER_ADDED,
            CHARACTER_REMOVED

        }

    }

}