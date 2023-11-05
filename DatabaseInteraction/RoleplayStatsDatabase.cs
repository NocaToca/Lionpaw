

using Newtonsoft.Json;

namespace Roleplay{

    //This is probably the most robust database, and we have a bunch of helper classes to store information
    //User_id will be the folder name.
    /*
        //Replies count (should be 1 until we add it)
        public int number_of_replies; DONE

        //We need the number of words to get an accurate average
        public int number_of_words; DONE
        public double average_wordlength; DONE

        //This is going to 1 until we add a roleplay statistic to another (or it's a loaded one)
        public double average_reply_length; DONE

        //This is going to be found with the datastructure from Vocabulary Robustness
        public string[] favorite_words = new string[3]; DONE

        //Their longest message - just compare number of words
        public long longest_message_length; DONE

        //Alliteration stuff
        public Dictionary<char, long> alliteration_counter = new Dictionary<char, long>(); DONE
        public double alliteration_score; DONE

        //Palindromes
        public List<string> palindromes = new List<string>(); DONE

        //Puncuation
        public Dictionary<char, long> punctionation_counter = new Dictionary<char, long>(); DONE

        //This is going to be the biggest list by far, but it holds every word used and a counter
        public Dictionary<string, long> word_counter = new Dictionary<string, long>(); DONE

        //This is our words/word_counter.Count
        public double average_unique_words; DONE

        //Find their name, incremenent it
        public Dictionary<string, long> favorite_character = new Dictionary<string, long>(); DONE
    */

    public class PuncuationData{
        [JsonProperty("puncuation_counter")]
        public List<PuncuationCounter> puncuation_counter = new List<PuncuationCounter>();

        public class PuncuationCounter{
            [JsonProperty("puncuations")]
            public char puncuations;

            [JsonProperty("amount")]
            public long amount;

            public PuncuationCounter(char puncuations, long amount){
                this.puncuations = puncuations;
                this.amount = amount;
            }
        }
        [JsonConstructor]
        public PuncuationData(List<PuncuationCounter> puncuation_counter){
            this.puncuation_counter = puncuation_counter;
        }

        public PuncuationData(Dictionary<char, long> puncuation_counter){
            foreach(KeyValuePair<char, long> pair in puncuation_counter){
                this.puncuation_counter.Add(new PuncuationCounter(pair.Key, pair.Value));
            }
        }

    }


    public class AlliterationData{
        [JsonProperty("score")]
        public double alliteration_score;

        [JsonProperty("alliteration_counter")]
        public List<AlliterationCounter> alliteration_counter = new List<AlliterationCounter>();

        public class AlliterationCounter{
            [JsonProperty("character")]
            public char character;
            
            [JsonProperty("amount")]
            public long amount;

            [JsonConstructor]
            public AlliterationCounter(char character, long amount){
                this.character = character;
                this.amount = amount;
            }
        }
        [JsonConstructor]
        public AlliterationData(double alliteration_score, List<AlliterationCounter> alliteration_counter){
            this.alliteration_counter = alliteration_counter;
            this.alliteration_score = alliteration_score;
        }

        public AlliterationData(double alliteration_score, Dictionary<char, long> alliteration_counter){
            foreach(KeyValuePair<char, long> pair in alliteration_counter){
                this.alliteration_counter.Add(new AlliterationCounter(pair.Key, pair.Value));
            }
            this.alliteration_score = alliteration_score;
        }


    }

    //Data that doesn't require anything else
    //Handles number_of_replies and longest_message_length
    public class FreeData{
        [JsonProperty("number_of_replies")]
        public int number_of_replies;

        [JsonProperty("longest_message")]
        public long longest_message;

        [JsonProperty("average_reply_length")]
        public double average_reply_length;
        [JsonConstructor]
        public FreeData(int number_of_replies, long longest_message, double average_reply_length){
            this.number_of_replies = number_of_replies;
            this.longest_message = longest_message;
            this.average_reply_length = average_reply_length;
        }
    }

    //Stores all of our word counts
    //Handles average_unique_words, word_counter, number_of_words
    public class WordStorage{

        [JsonProperty("average_unique_words")]
        public double average_unique_words;

        [JsonProperty("number_of_words")]
        public long number_of_words;

        [JsonProperty("word_counter")]
        public List<WordCounter> word_counters = new List<WordCounter>();

        [JsonProperty("favorite_words")]
        public string[] favorite_words = new string[3];

        [JsonProperty("palindromes")]
        public List<string> palindromes = new List<string>();

        public class WordCounter{
            [JsonProperty("word")]
            public string word;
            
            [JsonProperty("amount")]
            public long amount;

            [JsonConstructor]
            public WordCounter(string word, long amount){
                this.word = word;
                this.amount = amount;
            }
        }
        [JsonConstructor]
        public WordStorage(double average_unique_words, long number_of_words, List<WordCounter> word_counters, string[] favorite_words, List<string> palindromes){
            this.average_unique_words = average_unique_words;
            this.number_of_words = number_of_words;
            this.word_counters = word_counters;
            this.favorite_words = favorite_words;
            this.palindromes = palindromes;
        }

        public WordStorage(double average_unique_words, long number_of_words, Dictionary<string, long> word_counters, string[] favorite_words, List<string> palindromes){
            this.average_unique_words = average_unique_words;
            this.number_of_words = number_of_words;
            foreach(KeyValuePair<string, long> pair in word_counters){
                this.word_counters.Add(new WordCounter(pair.Key, pair.Value));
            }
            this.favorite_words = favorite_words;
            this.palindromes = palindromes;
        }
    }

    public class FavoriteCharacter{
        [JsonProperty("character_keys")]
        public List<CharacterKeys> character_counter = new List<CharacterKeys>();

        public class CharacterKeys{
            [JsonProperty("character")]
            public string character;
            [JsonProperty("amount")]
            public long amount;

            public CharacterKeys(string charcater, long amount){
                this.character = charcater;
                this.amount = amount;
            }

        }

        [JsonConstructor]
        public FavoriteCharacter(List<CharacterKeys> character_counter){
            this.character_counter = character_counter;
        }

        public FavoriteCharacter(Dictionary<string, long> favorite_character){
            foreach(KeyValuePair<string, long> pair in favorite_character){
                character_counter.Add(new CharacterKeys(pair.Key, pair.Value));
            }
        }

    }

    public static class RoleplayDatabase{

        public static object lock_object = new object();

        public static Tuple<FavoriteCharacter, WordStorage, FreeData, AlliterationData, PuncuationData> ExportData(RoleplayStatistics statistics){
            FavoriteCharacter favorite_character = new FavoriteCharacter(statistics.favorite_character);

            WordStorage word_storage = new WordStorage(statistics.average_unique_words,
            statistics.number_of_words,
            statistics.word_counter,
            statistics.favorite_words,
            statistics.palindromes);

            FreeData free_data = new FreeData(statistics.number_of_replies,
            statistics.longest_message_length,
            statistics.average_reply_length);

            AlliterationData alliteration_data = new AlliterationData(statistics.alliteration_score,
            statistics.alliteration_counter);

            PuncuationData puncuation_data = new PuncuationData(statistics.punctionation_counter);

            return new Tuple<FavoriteCharacter, WordStorage, FreeData, AlliterationData, PuncuationData>(favorite_character, word_storage, free_data, alliteration_data, puncuation_data);
        }

        public static void Save(RoleplayStatistics statistics, ulong id){

            //First, let's load the current statistics
            RoleplayStatistics? current_statics = LoadStats(id);

            string word_storage_path = "Database/"+id+"/word_storage.json";
            string free_data_path = "Database/"+id+"/free_data.json";
            string alliteration_data_path = "Database/"+id+"/alliteration_data.json";
            string punctuation_data_path = "Database/"+id+"/puncuation_data.json";
            string favorite_character_path = "Database/"+id+"/favorite_character.json";

            Tuple<FavoriteCharacter, WordStorage, FreeData, AlliterationData, PuncuationData> data;

            if(current_statics == null){
                //We'll just save everything new and then return;
                data = ExportData(statistics);
                //We don't worry about collisions, we're overwriting anyway
                SaveJSON<FavoriteCharacter>(data.Item1, favorite_character_path);
                SaveJSON<FreeData>(data.Item3, free_data_path);
                SaveJSON<AlliterationData>(data.Item4, alliteration_data_path);
                SaveJSON<WordStorage>(data.Item2, word_storage_path);
                SaveJSON<PuncuationData>(data.Item5, punctuation_data_path);
                
                return;
            }

            RoleplayStatistics resulting_stats = (RoleplayStatistics)current_statics + statistics;

            data = ExportData(resulting_stats);

            SaveJSON<FavoriteCharacter>(data.Item1, favorite_character_path);
            SaveJSON<FreeData>(data.Item3, free_data_path);
            SaveJSON<AlliterationData>(data.Item4, alliteration_data_path);
            SaveJSON<WordStorage>(data.Item2, word_storage_path);
            SaveJSON<PuncuationData>(data.Item5, punctuation_data_path);
                
            return;
        }

        public static RoleplayStatistics? LoadStats(ulong id){

            string word_storage_path = "Database/"+id+"/word_storage.json";
            string free_data_path = "Database/"+id+"/free_data.json";
            string alliteration_data_path = "Database/"+id+"/alliteration_data.json";
            string punctuation_data_path = "Database/"+id+"/puncuation_data.json";
            string favorite_character_path = "Database/"+id+"/favorite_character.json";

            try{
                return new RoleplayStatistics(
                    LoadJSON<FreeData>(free_data_path),
                    LoadJSON<FavoriteCharacter>(favorite_character_path),
                    LoadJSON<WordStorage>(word_storage_path),
                    LoadJSON<AlliterationData>(alliteration_data_path),
                    LoadJSON<PuncuationData>(punctuation_data_path)
                );
            }catch(Exception e){
                return null;
            }
            

        }

        public static T? LoadJSON<T>(string file_path){
            try{
                string json_data = File.ReadAllText(file_path);
                T? data = JsonConvert.DeserializeObject<T>(json_data);
                
                return data;
            }catch (Exception e){
                Console.WriteLine(e.Message);
                throw new Exception();
            }

        }

        public static void SaveJSON<T>(T data, string file_path){

            // Serialize the list back to JSON
            string updated_json_data = JsonConvert.SerializeObject(data);

            // Write the updated JSON data back to the file
            File.WriteAllText(file_path, updated_json_data);
        }

        public static bool IsUser(ulong id){
            string file_path = "Database/" + id.ToString();
            return Directory.Exists(file_path);
        }

        public static bool NewUser(ulong id){
            if(IsUser(id)){
                return false;
            }

            try{    
                string file_path = "Database/" + id.ToString();
                Directory.CreateDirectory(file_path);
                return true;

            } catch (Exception e){
                return false;
            }
        }

        public static bool Unregister(ulong id){
            try{
                string directoryPath = "Database/" + id.ToString();
                Directory.Delete(directoryPath, true);
                return true;
            }
            catch(Exception e){
                return false;
            }
        }

    }

}