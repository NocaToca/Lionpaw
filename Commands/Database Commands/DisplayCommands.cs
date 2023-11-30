using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lionpaw.Characters;
using Lionpaw.Databases;
using Lionpaw.Queries;

namespace Lionpaw{


    namespace Commands{

        public class ShowCommandsModule : Module{

            public ShowCommands show_command;

            public ShowCommandsModule(Guild guild) : base(guild){
                show_command = new ShowCommands(this);
            }

            [SlashCommandGroup("show", "The commands to display characters")]
            public class ShowCommands : ApplicationCommandModule{

                public ShowCommandsModule module;
                #pragma warning disable 8618
                public ShowCommands(){

                }
                #pragma warning restore

                public ShowCommands(ShowCommandsModule parent){
                    this.module = parent;
                }

                private async Task ShowList(InteractionContext ctx, int page, List<Character> characters, DiscordEmbedBuilder embed){
                    int page_number;
                    int amount_of_cats_to_display;
                    page_number = (page > (int)Math.Ceiling(characters.Count / 15.0)) ? page % (int)Math.Ceiling(characters.Count / 15.0) : page;
                    amount_of_cats_to_display = (page_number * 15 > characters.Count) ? characters.Count - ((page_number - 1) * 15) : 15;

                    string description = "";
                    for(int i = (page-1) * 15; i < amount_of_cats_to_display; i++){
                        description += characters[i].GetShortString() + "\n";
                    }

                    embed.WithDescription(description);
                    embed.WithFooter($"Page {page}/{Math.Ceiling(characters.Count/15.0)}");

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                }


                private async Task ShowAllFunction(InteractionContext ctx, int page){
                    List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(module.parent.id));

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                        Title = "Characters of " + ctx.Guild.Name
                    };

                    await ShowList(ctx, page, characters, embed);
                }

                [SlashCommand("all", "Shows all characters within the server")]
                public static async Task ShowAll(InteractionContext ctx, [Option("page", "The page number to go to.")] int page = 1){
                    await Lionpaw.MainBot.CallCommandForGuild<ShowCommands>(ctx).ShowAllFunction(ctx, page);
                }

                private async Task ShowUserFunction(InteractionContext ctx, DiscordUser user, int page){
                    List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(module.parent.id));
                    List<Character> filtered_characters = new List<Character>();

                    foreach(Character character in characters){
                        if(character.user_id == user.Id){
                            filtered_characters.Add(character);
                        }
                    }

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                        Title = "Characters beloning to " + user.Username
                    };

                    await ShowList(ctx, page, filtered_characters, embed);

                }

                [SlashCommand("user", "Shows all of the characters beloning to the user")]
                public static async Task ShowUser(InteractionContext ctx, 
                [Option("user", "Discord user to filter for")] DiscordUser user,
                [Option("page", "The page to filter to.")] int page = 1){
                    await Lionpaw.MainBot.CallCommandForGuild<ShowCommands>(ctx).ShowUserFunction(ctx, user, page);
                }

                private async Task ShowCharacterFunction(InteractionContext ctx, string name){
                    List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(module.parent.id));

                    Character? character_to_show = null;
                    foreach(Character character in characters){
                        if(character.name.ToLower() == name.ToLower()){
                            character_to_show = character;
                        }
                    }

                    //This really shouldn't happen because of our choice provider
                    if(character_to_show == null){
                        throw new Exception();
                    }

                    DiscordInteractionResponseBuilder response;
                    character_to_show.BuildInteraction(ctx.Guild, out response);

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, response);
                }

                [SlashCommand("character", "Shows detailed information about a specific character")]
                public static async Task ShowCharacter(InteractionContext ctx,
                [ChoiceProvider(typeof(Lionpaw.CharacterChoiceProvider))]
                [Option("name", "The name of the character to filter for")] string name){
                    await Lionpaw.MainBot.CallCommandForGuild<ShowCommands>(ctx).ShowCharacterFunction(ctx, name);
                }

                private async Task ShowFilteredFunction(InteractionContext ctx, string token_value, int page){

                    List<Query> queries = module.parent.GetQueries();
                    Query query = queries[0];
                    foreach(Query q in queries){
                        if(q.tokens != null){
                            foreach(string token in q.tokens){
                                if(token_value == token){
                                    query = q;
                                    break;
                                }
                            }
                        }
                    }

                    List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(module.parent.id));
                    List<Character> filtered_characters = new List<Character>();

                    foreach(Character character in characters){
                        foreach(Tuple<Query, object> parameters in character.parameters){
                            if(parameters.Item1 == query){
                                if(((string)parameters.Item2).ToLower() == token_value.ToLower()){
                                    filtered_characters.Add(character);
                                }
                            }
                        }
                    }

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                        Title = "Characters filtered for " + token_value
                    };

                    await ShowList(ctx, page, filtered_characters, embed);

                }

                //This command only considers tokened queries
                [SlashCommand("filtered", "Shows characters filtered by a parameter")]
                public static async Task ShowFiltered(InteractionContext ctx,
                [ChoiceProvider(typeof(Lionpaw.QueryOptionChoiceProvider))]
                [Option("parameter", "The parameter to filter for")] string token,
                [Option("page", "The page to go to")]int page = 1){
                    await Lionpaw.MainBot.CallCommandForGuild<ShowCommands>(ctx).ShowFilteredFunction(ctx, token, page);
                }


            }

        }
        
    }

}