

using System.Diagnostics;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

public static class Logger{

    public static object lock_object = new object();

    public static string file_path = "Logger/log.txt";

    public static void Log(string content){

        string writing_string = "LOG (" + DateTime.Now +"): "+ content;
        
        //We're going to be having LOTs of processes trying to log, so we're locking it here
        lock(lock_object){  
            File.AppendAllLines (file_path, new string[]{writing_string + "\n"});
        }
    }

    public static void Flush(){
        lock(lock_object){
            File.WriteAllText(file_path, "");
        }
    }
    
    public static void Error(string content){
        string writing_string = "ERROR (" + DateTime.Now +"): "+ content;
        
        //We're going to be having LOTs of processes trying to log, so we're locking it here
        lock(lock_object){  
            File.WriteAllLines(file_path, new string[]{writing_string + "\n"});
        }
    }

    public static void LogCommand(string command_name, InteractionContext ctx, Status status){
        string guild_name;

        if(ctx.Channel.GuildId == null){
            guild_name = "DMs";
        } else {
            guild_name = $"{ctx.Guild.Name}";
        }

        string user = $"{ctx.User.Username} ({ctx.User.Id})";

        string content = $"User {user} used command {command_name} in {guild_name} with status {status.ToString()}.";

        Log(content);
    }

    public static void LogCommand(string command_name, InteractionContext ctx, Status status, Stopwatch watch){
        string guild_name;

        if(ctx.Channel.GuildId == null){
            guild_name = "DMs";
        } else {
            guild_name = $"{ctx.Guild.Name}";
        }

        string user = $"{ctx.User.Username} ({ctx.User.Id})";

        string content = $"User {user} used command {command_name} in {guild_name} with status {status.ToString()}. Took {watch.ElapsedMilliseconds} milliseconds";

        Log(content);
    }

}