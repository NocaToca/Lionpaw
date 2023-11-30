
using System.ComponentModel;
using System.Diagnostics;
using DSharpPlus.SlashCommands;

namespace Lionpaw{

    public enum Status{

        
    }

    public static class Logger{

        private static object lock_object = new object();

        public static ulong MASTER_GUILD = 0;

        public static string file_path = "Logger/master_log.txt";
        public static string error_path = "Logger/error_log.txt";
        public static string GUILD_PATH(ulong id){return $"Logger/{id}/log.txt";}
    
        public static Task Log(string content, ulong guild_id){

            string file_path = (guild_id == MASTER_GUILD) ? Logger.file_path : GUILD_PATH(guild_id); 

            if(guild_id != MASTER_GUILD && !Directory.Exists($"Logger/{guild_id}")){
                Directory.CreateDirectory($"Logger/{guild_id}");
            }

            string writing_string = "LOG (" + DateTime.Now +"): "+ content;
            
            //We're going to be having LOTs of processes trying to log, so we're locking it here
            lock(lock_object){  
                File.AppendAllLines (file_path, new string[]{writing_string + "\n"});
            }

            return Task.CompletedTask;
        }

        public static Task Flush(ulong guild_id){
            string file_path = (guild_id == MASTER_GUILD) ? Logger.file_path : GUILD_PATH(guild_id); 

            lock(lock_object){
                File.WriteAllText(file_path, "");
            }

            return Task.CompletedTask;
        }
        
        public static Task Error(string content){
            
            string writing_string = "ERROR (" + DateTime.Now +"): "+ content;
            
            //We're going to be having LOTs of processes trying to log, so we're locking it here
            lock(lock_object){  
                File.WriteAllLines(error_path, new string[]{writing_string + "\n"});
            }

            return Task.CompletedTask;
        }

        public static Task LogCommand(string command_name, InteractionContext ctx){
            string guild_name;
            ulong guild_id = Logger.MASTER_GUILD;

            if(ctx.Channel.GuildId == null){
                guild_name = "DMs";
            } else {
                guild_name = $"{ctx.Guild.Name}";
                guild_id = (ulong)ctx.Channel.GuildId;
            }

            string user = $"{ctx.User.Username} ({ctx.User.Id})";

            string content = $"User {user} used command {command_name} in {guild_name}.";

            Log(content, guild_id);
            
            return Task.CompletedTask;
        }

        public static Task LogCommand(string command_name, InteractionContext ctx, Status status, Stopwatch watch){
            string guild_name;
            ulong guild_id = Logger.MASTER_GUILD;

            if(ctx.Channel.GuildId == null){
                guild_name = "DMs";
            } else {
                guild_name = $"{ctx.Guild.Name}";
                guild_id = (ulong)ctx.Channel.GuildId;
            }

            string user = $"{ctx.User.Username} ({ctx.User.Id})";

            string content = $"User {user} used command {command_name} in {guild_name} with status {status.ToString()}. Took {watch.ElapsedMilliseconds} milliseconds";

            Log(content, guild_id);

            return Task.CompletedTask;
        }

    }


}