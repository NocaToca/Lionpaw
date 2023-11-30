using System.Drawing;
using DSharpPlus.SlashCommands;
using Lionpaw.Characters;
using Lionpaw.Charts;
using Lionpaw.Databases;

namespace Lionpaw{


    namespace Commands{

        public class VisualModule : Module{

            public ChartCommands chart_commands;

            public VisualModule(Guild guild) : base(guild){
                chart_commands = new ChartCommands(this);
            }

            [SlashCommandGroup("chart", "Display informative charts about your server!")]
            public class ChartCommands : ApplicationCommandModule{

                public Module module;

                public ChartCommands(Module module){
                    this.module = module;
                }
                #pragma warning disable 8618
                public ChartCommands(){}
                #pragma warning restore

                [SlashCommand("pie", "Creates a pie chart filtered for the specific data!")]
                public static async Task PieChart(InteractionContext ctx, 
                [ChoiceProvider(typeof(Lionpaw.QueryChoiceProvider))]
                [Option("parameter", "The parameter to filter for")] string parameter){

                    await Lionpaw.MainBot.CallCommandForGuild<ChartCommands>(ctx)._PieChart(ctx, parameter);

                }

                private async Task _PieChart(InteractionContext ctx, string parameter){
                    List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(ctx.Guild.Id));

                    List<string> data = new List<string>();
                    foreach(Character character in characters){
                        #pragma warning disable 8604
                        data.Add(character.GetParameterValue(parameter).ToString());
                        #pragma warning restore
                    }

                    try{

                        Color[] colors = new Color[]{Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Pink, Color.Brown, Color.Teal, Color.Coral, Color.Violet};

                        if(!OperatingSystem.IsWindows()){
                            throw new Exception();
                        }
                        
                        Piechart<string> chart = new Piechart<string>(data, colors);
                        Piechart<string>.SaveChart(chart.BuildChart());

                        using(FileStream stream = new FileStream(Piechart<string>.file_name, FileMode.Open)){
                            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent(chart.GetKey((s) => {return s;})).AddFile(Piechart<string>.file_name,
                            stream));
                        }
                        
                    } catch (Exception e){
                        await Logger.Error(e.Message);
                    }


                }

            }

        }
        
    }

}