
internal class Launch{

    static void Main(string[] args){
        DocsHandler.Run();
        SlidesHandler.Run();
        RunBot();
    }
    static void RunBot(){
        // //The bot declaration
        Bot bot = new Bot();
        
        // //Runs the actual bot
        bot.RunBotAsync().GetAwaiter().GetResult();
    }
}
