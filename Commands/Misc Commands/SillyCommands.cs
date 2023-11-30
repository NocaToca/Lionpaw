using DSharpPlus.SlashCommands;

namespace Lionpaw{


    namespace Commands{

        public class SillyModule : Module{

            public SillyCommands silly_commands;

            public SillyModule(Guild guild) : base(guild){
                silly_commands = new SillyCommands();
            }

            public class SillyCommands : ApplicationCommandModule{

                public SillyModule module;

                #pragma warning disable 8618
                public SillyCommands(){

                }
                #pragma warning restore

                public SillyCommands(SillyModule module){
                    this.module = module;
                }

                
                [SlashCommand("8ball", "Ask an 8 ball question to a character :3")]
                public static async Task AskEightBall(InteractionContext ctx,
                [ChoiceProvider(typeof(Lionpaw.CharacterChoiceProvider))]
                [Option("character", "Character to ask the question to.")] string name,
                [Option("question","Question to ask!")] string question){
                    int seed = DateTime.Now.Millisecond;

                    int Randomize(char c, int seed){
                        int operation = (c+seed) % 6;
                        if(operation == 0){
                            seed += c;
                        } else 
                        if(operation == 1){
                            seed -= c;
                        } else
                        if(operation == 2){
                            seed = (c == 0) ? seed : seed/c;
                        } else
                        if(operation == 3){
                            seed *= c;
                        } else
                        if(operation == 4){
                            seed = seed << (c % 2);
                        } else
                        if(operation == 5){
                            seed = seed >> (c % 2);
                        }
                        return seed;
                    }

                    //We're going to use the cat's name and the question to generate a random seed.
                    
                    foreach(char c in name.ToCharArray()){
                        seed = Randomize(c, seed);
                    }
                    foreach(char c in question.ToCharArray()){
                        seed = Randomize(c, seed);
                    }

                    int answer = seed % 20;
                    string [] answers = new string[]{"It is certain",	"Reply hazy, try again",	"Don't count on it", "It is decidedly so",	"Ask again later",	"My reply is no", "Without a doubt",	"Better not tell you now",	"My sources say no",
                                                    "Yes definitely",	"Cannot predict now",	"Outlook not so good", "You may rely on it",	"Concentrate and ask again",	"Very doubtful",
                                                    "As I see it, yes",	"Most likely",	"Outlook good",	"Yes",	"Signs point to yes"	};
                    await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent("Here is the answer I got from " + name + "! They said:\n\n" +
                    $"{name}: \"{answers[answer]}\""));

                }


            }

        }
        
    }

}