using System.Drawing;
using DSharpPlus.SlashCommands;

public class ChartCommands : ApplicationCommandModule{

    public enum PieOptions{
        [ChoiceName("Gender")]
        Gender,
        [ChoiceName("Rank")]
        Rank,
        [ChoiceName("Clan")]
        Clan
    }

    [SlashCommand("piechart", "Creates a pie chart showcasing the related data!")]
    public async Task Piechart(InteractionContext ctx,
    [Option("type", "What type of data to display!")] PieOptions options){

        if(options == PieOptions.Gender){
            await ShowChart<Gender>(ctx, cat => (Gender)cat.GetGender());
            
        } else 
        if(options == PieOptions.Clan){
            await ShowChart<Clan>(ctx, cat => (Clan)cat.GetClan());
        } else
        if(options == PieOptions.Rank){
            await ShowChart<Rank>(ctx, cat => (Rank)cat.GetRank());
        }
    }

    private async Task ShowChart<T>(InteractionContext ctx, Func<Cat,T> grab_data){
        try{
            List<Cat> cats = DatabaseReader.LoadCats();

            List<T> data = new List<T>();
            foreach(Cat cat in cats){
                try{
                    T? data_shard = grab_data(cat);
                    if(data_shard != null){
                        data.Add((T)data_shard);
                    } 
                }catch(Exception e){

                }
            }

            Color[] colors = new Color[]{Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Orange, Color.Pink, Color.Brown, Color.Teal, Color.Coral, Color.Violet};
            
            Piechart<T> chart = new Piechart<T>(data, colors);
            Piechart<T>.SaveChart(chart.BuildChart());

            using(FileStream stream = new FileStream(Piechart<T>.file_name, FileMode.Open)){
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder().WithContent(chart.GetKey()).AddFile(Piechart<T>.file_name,
                stream));
            }
            
        } catch (Exception e){
            Logger.Error(e.Message);
        }
        
    }

}