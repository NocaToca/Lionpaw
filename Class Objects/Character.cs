

using DSharpPlus.Entities;
using Lionpaw.Queries;
using Newtonsoft.Json;

namespace Lionpaw{

    namespace Characters{

        public class Character{

            [JsonProperty("parameters")]
            public List<Tuple<Query, object>> parameters;

            [JsonProperty("user")]
            public ulong user_id;

            [JsonProperty("available")]
            public bool available;

            [JsonProperty("guild_id")]
            public ulong guild_id;

            [JsonProperty("link")]
            public string link;

            public string name {get{
                string _name = "Character";
                foreach(Tuple<Query, object> parameter in parameters){
                    //Uhhh, I'll handle more name cases as they come and go
                    if(parameter.Item1.name.ToLower() == "name"){
                        _name = (string)parameter.Item2;
                    }
                }
                return _name;
            }}


            public Character(List<QueryResult> results, ulong user_id, ulong guild_id, string link){
                parameters = new List<Tuple<Query, object>>();
                foreach(QueryResult result in results){
                    parameters.Add(new Tuple<Query, object>(result.query, result.result));
                }

                this.user_id = user_id;
                available = true;
                this.guild_id = guild_id;
                this.link = link;
            }

            [JsonConstructor]
            private Character(List<Tuple<Query, object>> parameters, ulong user_id, bool available, ulong guild_id, string link){
                this.parameters = parameters;
                this.user_id = user_id;
                this.available = available;
                this.guild_id = guild_id;
                this.link = link;
            }

            public DiscordEmbed BuildEmbed(DiscordGuild guild){
                //First let's see if there is a name parameter
                string name = "Character of " + guild.Name;
                foreach(Tuple<Query, object> parameter in parameters){
                    //Uhhh, I'll handle more name cases as they come and go
                    if(parameter.Item1.name.ToLower() == "name"){
                        name = (string)parameter.Item2;
                    }
                }
                name = "**✧⋄⋆⋅⋆⋄"+ name +"⋄⋆⋅⋆⋄✧**";

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder{
                    Title = name
                };

                foreach(Tuple<Query, object> parameter in parameters){
                    embed.AddField("__" + parameter.Item1.name +"__", (string)parameter.Item2); //I'll flesh this out more depending on display properties
                }

                embed.WithFooter($"Belongs to {guild.GetMemberAsync(user_id).Result.Nickname}");

                return embed.Build();
            }

            public void BuildInteraction(DiscordGuild guild, out DiscordInteractionResponseBuilder response){
                response = new DiscordInteractionResponseBuilder();
                DiscordComponent[] components = new DiscordComponent[]{
                    new DiscordLinkButtonComponent(link, name)
                };
                response.AddComponents(components);
                response.AddEmbed(BuildEmbed(guild));
            }

            public string GetShortString(){
                string return_string ="**__"+ name+"__**";
                return_string += " | Available: " + available;

                return return_string;
            }

            public void Edit(string parameter, string new_value){
                List<Tuple<Query, object>> new_parameters = new List<Tuple<Query, object>>();

                foreach(Tuple<Query, object> tuple in parameters){
                    if(tuple.Item1.name.ToLower() == parameter){
                        new_parameters.Add(new Tuple<Query, object>(tuple.Item1, new_value));
                    } else {
                        new_parameters.Add(tuple);
                    }
                }

                parameters = new_parameters;
            }

            public bool HasToken(string token){
                foreach(Tuple<Query, object> param in parameters){
                    if((string)param.Item2 == token){
                        return true;
                    }
                }
                return false;
            }

            public override bool Equals(object? obj){
                if(obj is Character other){
                    //We're going to check only two things, then assume the rest
                    bool equal = false;
                    equal = other.name == name;
                    equal = equal && other.user_id == user_id;
                    return equal;
                }

                return false;
            }


            public object GetParameterValue(string parameter){
                foreach(Tuple<Query, object> query in parameters){
                    if(query.Item1.name == parameter){
                        return query.Item2;
                    }
                }

                throw new Exception();
            }

            public override int GetHashCode(){
                return base.GetHashCode();
            }
        }
        
    }

}