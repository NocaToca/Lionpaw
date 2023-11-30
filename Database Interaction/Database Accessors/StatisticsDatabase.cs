

using Lionpaw.Statistics;

namespace Lionpaw{

    namespace Databases{

        public static class StatisticsAccessor{

            static string free_data_path  (ulong user_id, ulong guild_id) {return DatabaseAccessor.FREE_DATA_PATH(user_id, guild_id);}
            static string favorite_character_path  (ulong user_id, ulong guild_id) { return DatabaseAccessor.FAVORITE_CHARACTER_PATH(user_id, guild_id);}
            static string alliteration_path  (ulong user_id, ulong guild_id) { return DatabaseAccessor.ALLITERATION_DATA_PATH(user_id, guild_id);}
            static string word_path  (ulong user_id, ulong guild_id) { return DatabaseAccessor.WORD_STORAGE_PATH(user_id, guild_id);}
            static string punctuation_path (ulong user_id, ulong guild_id) { return DatabaseAccessor.PUNCTUATION_DATA_PATH(user_id, guild_id);}

            public async static Task SaveStatistics(RoleplayStatistics statistics, ulong user_id, ulong guild_id){

                RoleplayStatistics? current_statistics = await LoadStatistics(user_id, guild_id);

                if(current_statistics != null){
                    statistics += (RoleplayStatistics)current_statistics;
                }

                //The first think we need to do is extract each storage class:
                FreeData free_data = (FreeData)statistics.ExtractData(new FreeData());
                FavoriteCharacter favorite_character = (FavoriteCharacter)statistics.ExtractData(new FavoriteCharacter());
                AlliterationData alliteration_data = (AlliterationData)statistics.ExtractData(new AlliterationData());
                WordStorage word_storage = (WordStorage)statistics.ExtractData(new WordStorage());
                PuncuationData puncuation_data = (PuncuationData)statistics.ExtractData(new PuncuationData());

                

                await DatabaseAccessor.Save<FreeData>(free_data, free_data_path(user_id, guild_id));
                await DatabaseAccessor.Save<FavoriteCharacter>(favorite_character, favorite_character_path(user_id, guild_id));
                await DatabaseAccessor.Save<AlliterationData>(alliteration_data, alliteration_path(user_id, guild_id));
                await DatabaseAccessor.Save<WordStorage>(word_storage, word_path(user_id, guild_id));
                await DatabaseAccessor.Save<PuncuationData>(puncuation_data, punctuation_path(user_id, guild_id));
            }

            public async static Task<RoleplayStatistics?> LoadStatistics(ulong user_id, ulong guild_id){
                try{
                    FreeData free_data = await DatabaseAccessor.LoadItem<FreeData>(free_data_path(user_id, guild_id));
                    FavoriteCharacter favorite_character = await DatabaseAccessor.LoadItem<FavoriteCharacter>(favorite_character_path(user_id, guild_id));
                    AlliterationData alliteration_data = await DatabaseAccessor.LoadItem<AlliterationData>(alliteration_path(user_id, guild_id));
                    WordStorage word_storage = await DatabaseAccessor.LoadItem<WordStorage>(word_path(user_id, guild_id));
                    PuncuationData punctuation_data = await DatabaseAccessor.LoadItem<PuncuationData>(punctuation_path(user_id, guild_id));

                    return new RoleplayStatistics(free_data, favorite_character, word_storage, alliteration_data, punctuation_data);
                } catch{
                    return null;
                }
            }

            public async static Task<bool> IsUser(ulong user_id){
                return await DatabaseAccessor.DirectoryExist(DatabaseAccessor.STATISTICS_PATH(user_id));
            }

            public async static Task<RoleplayStatistics?> LoadAll(ulong user_id){
                //We're just going to have to go through each directory
                string base_file_path = "Database/Users/"+user_id;

                List<RoleplayStatistics> statistics_list = new List<RoleplayStatistics>();

                string[] user_directories = Directory.GetDirectories(base_file_path);
                foreach(string directory_string in user_directories){
                    string[] split_directory = directory_string.Split("/");
                    ulong guild_id = ulong.Parse(split_directory[split_directory.Length-1]);

                    RoleplayStatistics? statistics = await LoadStatistics(user_id, guild_id);
                    if(statistics == null){
                        continue;
                    }
                    statistics_list.Add((RoleplayStatistics)statistics);
                }

                if(statistics_list.Count == 0){
                    return null;
                }

                //Then we're going to add
                RoleplayStatistics result = statistics_list[0]; 
                for(int i = 1; i < statistics_list.Count; i++){
                    result += statistics_list[i];
                }

                return result;
            }

        }

    }

}