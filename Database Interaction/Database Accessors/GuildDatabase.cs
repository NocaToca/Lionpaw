namespace Lionpaw{

    namespace Databases{

        public static class GuildAccessor{

            //Helper for loading all guilds
            public static async Task<Guild[]> LoadAllGuilds(){
                string base_file_path = "Database/Guilds";


                string[] guild_directories = Directory.GetDirectories(base_file_path);

                List<Task<Guild>> tasks = new List<Task<Guild>>();
                foreach(string directory in guild_directories){
                    tasks.Add(LoadGuild(directory + "/guild.json"));
                }

                return await Task.WhenAll(tasks);

            }

            public static async Task<Guild> LoadGuild(string file_path){
                return await DatabaseAccessor.LoadItem<Guild>(file_path);
            }

            public static async Task RegisterGuild(Guild guild){
                //We can't await this call because we need the directory
                await DatabaseAccessor.CreateDirectory(DatabaseAccessor.GUILD_PATH(guild.id));

                await DatabaseAccessor.Save<Guild>(guild, DatabaseAccessor.GUILD_SAVE_PATH(guild.id));

                await Logger.Log("Registered", 0);

            }

        }

    }

}