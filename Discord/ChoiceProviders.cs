using System.Drawing;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

public class CatChoiceProvider : IChoiceProvider{

    public static List<DiscordApplicationCommandOptionChoice> choices = new List<DiscordApplicationCommandOptionChoice>();

    //Called whenever we add, change, or remove a cat from the database
    public static void RefreshDatabase(){
        choices.Clear();

        List<Cat> cats = DatabaseReader.LoadCats();
        foreach(Cat cat in cats){
            choices.Add(new DiscordApplicationCommandOptionChoice(cat.GetName(), cat.GetName()));
        }
    }
    public async Task<IEnumerable<DiscordApplicationCommandOptionChoice>> Provider(){

        RefreshDatabase();

        return choices;
    }

    

}