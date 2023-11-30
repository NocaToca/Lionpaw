

using System.ComponentModel;
using System.Net.Http.Headers;
using System.Speech.Synthesis;
using Lionpaw.Characters;
using Newtonsoft.Json;

namespace Lionpaw{

    namespace Databases{

        public static class DatabaseAccessor{

            public static string GUILD_PATH(ulong id){return $"Database/Guilds/{id}";}
            public static string GUILD_SAVE_PATH(ulong id){return GUILD_PATH(id) + "/guild.json";}
            public static string CHARACTER_PATH(ulong guild_id){return GUILD_PATH(guild_id) + "/characters.json";}

            public static string STATISTICS_PATH(ulong id){return $"Database/Users/{id}";}

            public static string WEATHER_SAVE_DATA_PATH(ulong guild_id){return $"Database/Guilds/{guild_id}/weather_save_data.json";}

            public static string WEATHER_DATA_PATH(ulong guild_id){return $"Database/Guilds/{guild_id}/weather_data.json";}

            public static string ALLITERATION_DATA_PATH(ulong user, ulong guild){return $"Database/Users/{user}/{guild}/alliteration_data.json";}
            public static string FREE_DATA_PATH(ulong user, ulong guild){return $"Database/Users/{user}/{guild}/free_data.json";}
            public static string WORD_STORAGE_PATH(ulong user, ulong guild){return $"Database/Users/{user}/{guild}/word_storage.json";}
            public static string PUNCTUATION_DATA_PATH(ulong user, ulong guild){return $"Database/Users/{user}/{guild}/punctuation_data.json";}
            public static string FAVORITE_CHARACTER_PATH(ulong user, ulong guild){return $"Database/Users/{user}/{guild}/favorite_character.json";}

            public static object lock_object = new object();

            public static Task<bool> CreateDirectory(string directory_name){
                if(DirectoryExist(directory_name).Result){
                    return Task.FromResult(false);
                }

                try{
                    lock(lock_object){
                        Directory.CreateDirectory(directory_name);
                    }
                    return Task.FromResult(true);
                }catch(Exception e){
                    Logger.Error(e.Message);
                    return Task.FromResult(false);
                }
            }

            public static Task<bool> DirectoryExist(string directory_name){
                lock(lock_object){
                    return Task.FromResult(Directory.Exists(directory_name));
                }
            }

            public static Task<bool> DeleteDirectory(string directory_name){
                lock(lock_object){
                    try{
                        Directory.Delete(directory_name, true);
                        return Task.FromResult(true);
                    }catch(Exception e){
                        Logger.Error(e.Message);
                        return Task.FromResult(false);
                    }
                }
            }

            public static Task Delete<T>(T item, string file_path){
                try{
                    List<T> items = LoadItems<T>(file_path).Result;

                    #pragma warning disable 8602
                    items.RemoveAll(existing => existing.Equals(item));
                    #pragma warning restore

                    string json_data = JsonConvert.SerializeObject(items);

                    lock(lock_object){
                        File.WriteAllText(file_path, json_data);
                    }

                    return Task.CompletedTask;
                }catch (Exception e){
                    Logger.Error(e.Message);
                    return Task.FromException(e);
                }
            }

            public static Task SaveAppend<T>(T item, string file_path){
                try{
                    List<T> items = LoadItems<T>(file_path).Result;

                    items.Add(item);

                    string json_data = JsonConvert.SerializeObject(items);

                    lock(lock_object){
                        File.WriteAllText(file_path, json_data);
                    }

                    return Task.CompletedTask;
                }catch (Exception e){
                    Logger.Error(e.Message);
                    return Task.FromException(e);
                }
            }

            public static async Task SaveCreate<T>(T itme, string file_path){
                if(!await DirectoryExist(file_path)){
                    await CreateDirectory(file_path);
                }
                await Save(itme, file_path);
            }

            public static Task Save<T>(T item, string file_path){
                try{
                    string json_data = JsonConvert.SerializeObject(item);

                    lock(lock_object){
                        File.WriteAllText(file_path, json_data);
                    }

                    return Task.CompletedTask;
                }catch (Exception e){
                    Logger.Error(e.Message);
                    return Task.FromException(e);
                }
            } 

            public static Task SaveAll<T>(List<T> items, string file_path){
                foreach(T item in items){
                    SaveAppend(item, file_path);
                }
                return Task.CompletedTask;
            }

            public static Task<List<T>> LoadItems<T>(string file_path){
                lock(lock_object){
                    List<T> items = new List<T>();
                    try{
                        string json_data = File.ReadAllText(file_path);

                        #pragma warning disable 8600
                        items = JsonConvert.DeserializeObject<List<T>>(json_data);
                        #pragma warning restore

                        if(items == null){
                            throw new Exception();
                        }

                        return Task.FromResult(items);
                    }catch(Exception e){
                        Logger.Error(e.Message);
                        return Task.FromResult(new List<T>());
                    }
                }
            }

            public static Task<T> LoadItem<T>(string file_path){
                lock(lock_object){
                    T item;
                    try{
                        string json_data = File.ReadAllText(file_path);

                        #pragma warning disable 8600
                        item = JsonConvert.DeserializeObject<T>(json_data);
                        #pragma warning restore

                        if(item == null){
                            throw new Exception();
                        }

                        return Task.FromResult(item);
                    }catch(Exception e){
                        Logger.Error(e.Message);
                        throw;
                    }
                }
            }

        }
    }

}