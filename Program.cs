
internal class Launch{

    static void Main(string[] args){
        DocsHandler.Run();
        // string doc_id = DocsHandler.GetDocIdFromURL("https://docs.google.com/document/d/1idrJpOuf0umwLEloReMBqxcvsbcu9eVRBtQWXABnESc/edit");
        // List<string> strings = DocsHandler.GetAllText(doc_id);
        // strings = DocsHandler.FormatTextForParsing(strings);

        // foreach(string s in strings){
        //     Console.WriteLine(s);
        // }
        SlidesHandler.Run();

        //I'll have this here for database updates
        // List<Cat> cats = DatabaseReader.LoadCats();
        // foreach(Cat cat in cats){
        //     DatabaseWriter.SaveCat(cat);
        // }

        RunBot();
    }
    static void RunBot(){
        // //The bot declaration
        Bot bot = new Bot();
        
        // //Runs the actual bot
        bot.RunBotAsync().GetAwaiter().GetResult();
    }
}
