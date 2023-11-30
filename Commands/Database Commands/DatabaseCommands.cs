
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lionpaw.Parsing;
using Lionpaw.Characters;
using Lionpaw.Queries;
using Lionpaw.Databases;


namespace Lionpaw{
    namespace Commands{

        public class DatabaseCommandModule : Module{

            public AddCommand add_commands;
            public EditCommand edit_command;
            public RemoveCommand remove_command;

            public DatabaseCommandModule(Guild guild) : base(guild){
                add_commands = new AddCommand(this);
                edit_command = new EditCommand(this);
                remove_command = new RemoveCommand(this);
            }

            //Now I realize I could have made one group with:
            // /character add, /character edit, /character remove but I like /add character better

            [SlashCommandGroup("add", "Adds characters to the database")]
            public class AddCommand : ApplicationCommandModule{

                DatabaseCommandModule parent;
                #pragma warning disable 8618
                public AddCommand(){

                }
                #pragma warning restore
                
                internal AddCommand(DatabaseCommandModule commands){
                    parent = commands;
                }

                [SlashCommand("character", "Adds a character to the database!")]
                public static async Task Add(InteractionContext ctx,
                [Option("User", "The user this character belongs to")] DiscordUser user,
                [Option("Link", "External/Internal link to the detailed character description")] string link = "none"){
                    Character character = await Lionpaw.MainBot.CallCommandForGuild<AddCommand>(ctx).AddCharacter(ctx, user, link);
                    await Lionpaw.MainBot.OnCharacterAdded.Invoke(character, ctx.Guild, Lionpaw.MainBot);
                    await Lionpaw.MainBot.RefreshCommands();

                }

                public async Task<Character> AddCharacter(InteractionContext ctx, DiscordUser user, string link){
                    Character? character = null;

                    if(link == "none"){
                        //We're just going to add it

                    } else
                    if(GoogleHandler.IsGoogleLink(link)){
                        Character google_result = await GetCharacterFromGoogleLink(link, user);
                        character = google_result;
                    } else {
                        //I'll have to figure out what to do here
                    }

                    if(character == null){
                        throw new NotImplementedException();
                    }

                    DiscordInteractionResponseBuilder response;
                    character.BuildInteraction(ctx.Channel.Guild, out response);

                    await DatabaseAccessor.SaveAppend<Character>(character, DatabaseAccessor.CHARACTER_PATH(parent.parent.id));

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, response);

                    return character;
                }

                public async Task<Character> GetCharacterFromGoogleLink(string link, DiscordUser user){

                    List<string> pulled_text = GoogleHandler.GetAllText(link);

                    //Now we have to parse for each query we have
                    List<Query> queries = parent.parent.GetQueries();
                    
                    //Then we can grab the results
                    List<QueryResult> results = await Parser.GetQueryResults(queries, pulled_text);

                    Character character = new Character(results, user.Id, parent.parent.id, link);

                    return character;
                }

            }

            [SlashCommandGroup("edit", "For editing characters.")]
            public class EditCommand : ApplicationCommandModule{

                DatabaseCommandModule parent;
                #pragma warning disable 8618
                public EditCommand(){

                }
                #pragma warning restore
                
                internal EditCommand(DatabaseCommandModule commands){
                    parent = commands;
                }

                [SlashCommand("character", "For editing our character")]
                public static async Task Edit(InteractionContext ctx,
                [ChoiceProvider(typeof(Lionpaw.CharacterChoiceProvider))]
                [Option("character", "The character to edit")]string character,
                [ChoiceProvider(typeof(Lionpaw.QueryChoiceProvider))]
                [Option("parameter", "The parameter to edit")]string parameter,
                [Option("value", "The new value to edit to")]string value){
                    await Lionpaw.MainBot.CallCommandForGuild<EditCommand>(ctx).EditCharacter(ctx, character, parameter, value);
                    await Lionpaw.MainBot.RefreshCommands();

                }

                public async Task EditCharacter(InteractionContext ctx, string character, string parameter, string value){
                    List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(ctx.Guild.Id));
                    Character? character_to_edit = null;

                    foreach(Character _character in characters){
                        if(_character.name == character){
                            character_to_edit = _character;
                            break;
                        }
                    }

                    if(character_to_edit == null){
                        throw new Exception(); //This should really never happen
                    }

                    await Lionpaw.MainBot.OnCharacterRemoved(character_to_edit, ctx.Guild, Lionpaw.MainBot);
                    await DatabaseAccessor.Delete<Character>(character_to_edit, DatabaseAccessor.CHARACTER_PATH(parent.parent.id));


                    character_to_edit.Edit(parameter, value);

                    
                    await DatabaseAccessor.SaveAppend<Character>(character_to_edit, DatabaseAccessor.CHARACTER_PATH(parent.parent.id));
                    await Lionpaw.MainBot.OnCharacterAdded(character_to_edit, ctx.Guild, Lionpaw.MainBot);

                    DiscordInteractionResponseBuilder response;
                    character_to_edit.BuildInteraction(ctx.Guild, out response);

                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, response);

                }


            }

            [SlashCommandGroup("remove", "For removing characters.")]
            public class RemoveCommand : ApplicationCommandModule{
                DatabaseCommandModule parent;

                
                #pragma warning disable 8618
                public RemoveCommand(){
                    
                }
                #pragma warning restore
                
                internal RemoveCommand(DatabaseCommandModule commands){
                    parent = commands;
                }

                [SlashCommand("character", "Removes the character.")]
                public static async Task Remove(InteractionContext ctx,
                [ChoiceProvider(typeof(Lionpaw.CharacterChoiceProvider))]
                [Option("name", "The name of the character")] string name){
                    Character character = await Lionpaw.MainBot.CallCommandForGuild<RemoveCommand>(ctx).RemoveCharacter(name);
                    await Lionpaw.MainBot.OnCharacterRemoved.Invoke(character, ctx.Guild, Lionpaw.MainBot);
                    await Lionpaw.MainBot.RefreshCommands();

                }

                public async Task<Character> RemoveCharacter(string name){
                    List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(parent.parent.id));
                    foreach(Character character in characters){
                        if(character.name.ToLower() == name.ToLower()){
                            await DatabaseAccessor.Delete<Character>(character, DatabaseAccessor.CHARACTER_PATH(parent.parent.id));
                            return character;
                        }
                    }

                    //This should never happen
                    throw new Exception();
                }
            }


        }

    }
}