
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using Lionpaw.Characters;
using Lionpaw.Databases;
using Newtonsoft.Json;

namespace Lionpaw{

    namespace Statistics{

            public abstract class Data{
                public abstract Data Extract(RoleplayStatistics statistics);
            }

            public class PuncuationData : Data{
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

               public PuncuationData(){}

                public override Data Extract(RoleplayStatistics statistics){
                    foreach(KeyValuePair<char, long> pair in statistics.punctionation_counter){
                        this.puncuation_counter.Add(new PuncuationCounter(pair.Key, pair.Value));
                    }

                    return this;
                }

            }


            public class AlliterationData : Data{
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

                public AlliterationData(){}

                public override Data Extract(RoleplayStatistics statistics){
                    foreach(KeyValuePair<char, long> pair in statistics.alliteration_counter){
                        this.alliteration_counter.Add(new AlliterationCounter(pair.Key, pair.Value));
                    }
                    this.alliteration_score = statistics.alliteration_score;

                    return this;
                }
            }

            //Data that doesn't require anything else
            //Handles number_of_replies and longest_message_length
            public class FreeData : Data{
                [JsonProperty("number_of_replies")]
                public int number_of_replies;

                [JsonProperty("longest_message")]
                public long longest_message;

                [JsonProperty("average_reply_length")]
                public double average_reply_length;

                [JsonProperty("guild_id")] // The guild ID these replies belong to
                public ulong guild_id;

                [JsonConstructor]
                public FreeData(int number_of_replies, long longest_message, double average_reply_length, ulong guild_id){
                    this.number_of_replies = number_of_replies;
                    this.longest_message = longest_message;
                    this.average_reply_length = average_reply_length;
                    this.guild_id = guild_id;
                }

                public FreeData(){}

                public override Data Extract(RoleplayStatistics statistics){
                    number_of_replies = statistics.number_of_replies;
                    longest_message = statistics.longest_message_length;
                    average_reply_length = statistics.average_reply_length;
                    guild_id = statistics.guild_id;

                    return this;
                }
            }

            //Stores all of our word counts
            //Handles average_unique_words, word_counter, number_of_words
            public class WordStorage : Data{

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

                [JsonProperty("average_word_length")]
                public double average_wordlength;

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
                public WordStorage(double average_unique_words, long number_of_words, List<WordCounter> word_counters, string[] favorite_words, List<string> palindromes, double average_word_length){
                    this.average_unique_words = average_unique_words;
                    this.number_of_words = number_of_words;
                    this.word_counters = word_counters;
                    this.favorite_words = favorite_words;
                    this.palindromes = palindromes;
                    average_wordlength = average_word_length;
                }

                public WordStorage(){}

                public override Data Extract(RoleplayStatistics statistics){
                    this.average_unique_words = statistics.average_unique_words;
                    this.number_of_words = statistics.number_of_words;
                    foreach(KeyValuePair<string, long> pair in statistics.word_counter){
                        this.word_counters.Add(new WordCounter(pair.Key, pair.Value));
                    }
                    this.favorite_words = statistics.favorite_words;
                    this.palindromes = statistics.palindromes;
                    average_wordlength = statistics.average_wordlength;

                    return this;
                }
            }

            public class FavoriteCharacter : Data{
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

                public FavoriteCharacter(){}

                public override Data Extract(RoleplayStatistics statistics){
                    foreach(KeyValuePair<string, long> pair in statistics.favorite_character){
                        character_counter.Add(new CharacterKeys(pair.Key, pair.Value));
                    }

                    return this;
                }

            }


            public struct RoleplayStatistics{

        //Replies count (should be 1 until we add it)
        public int number_of_replies;

        //We need the number of words to get an accurate average
        public long number_of_words;
        public double average_wordlength;

        //This is going to 1 until we add a roleplay statistic to another (or it's a loaded one)
        public double average_reply_length;

        //This is going to be found with the datastructure from Vocabulary Robustness
        public string[] favorite_words = new string[3];

        //Their longest message - just compare number of words
        public long longest_message_length;

        //Alliteration stuff
        public Dictionary<char, long> alliteration_counter = new Dictionary<char, long>();
        public double alliteration_score;

        //Palindromes
        public List<string> palindromes = new List<string>();

        //Puncuation
        public Dictionary<char, long> punctionation_counter = new Dictionary<char, long>();

        //This is going to be the biggest list by far, but it holds every word used and a counter
        public Dictionary<string, long> word_counter = new Dictionary<string, long>();

        //This is our words/word_counter.Count
        public double average_unique_words;

        //Find their name, incremenent it
        public Dictionary<string, long> favorite_character = new Dictionary<string, long>();

        public ulong guild_id;

        public RoleplayStatistics(){
        }

        public RoleplayStatistics(FreeData? free_data, FavoriteCharacter? favorite_character, WordStorage? word_data, AlliterationData? alliteration_data, PuncuationData? puncuation_data){
            if(free_data != null){
                //Free Data
                this.number_of_replies = free_data.number_of_replies;
                this.average_reply_length = free_data.average_reply_length;
                this.longest_message_length = free_data.longest_message;
                this.guild_id = free_data.guild_id;
            }
            
            if(favorite_character != null){
                //Favorite Character
                foreach(FavoriteCharacter.CharacterKeys keys in favorite_character.character_counter){
                    this.favorite_character.Add(keys.character, keys.amount);
                }
            }
            
            if(word_data != null){
                //Word Storage
                foreach(WordStorage.WordCounter counter in word_data.word_counters){
                    this.word_counter.Add(counter.word, counter.amount);
                }
                this.average_unique_words = word_data.average_unique_words;
                this.number_of_words = word_data.number_of_words;
                this.palindromes = word_data.palindromes;
                this.favorite_words = word_data.favorite_words;
                this.average_wordlength = word_data.average_wordlength;
            }   
            
            if(alliteration_data != null){
                //Alliteration Data
                foreach(AlliterationData.AlliterationCounter counter in alliteration_data.alliteration_counter){
                    alliteration_counter.Add(counter.character, counter.amount);
                }
                this.alliteration_score = alliteration_data.alliteration_score;
            }
            
            if(puncuation_data != null){
                //Puncuation Data
                foreach(PuncuationData.PuncuationCounter counter in puncuation_data.puncuation_counter){
                    punctionation_counter.Add(counter.puncuations, counter.amount);
                }
            }
            
        }

        public static RoleplayStatistics operator +(RoleplayStatistics one, RoleplayStatistics two){

            //Defining Local functions to make this easy
            #pragma warning disable 8714
            Dictionary<T, long> AddDictionaries<T>(Dictionary<T, long> _one, Dictionary<T, long> _two){
                Dictionary<T, long> favorite_character = new Dictionary<T, long>();

                //We know the first one is going to be all new anyway
                foreach(KeyValuePair<T, long> pair in _one){
                    favorite_character.Add(pair.Key, pair.Value);
                }
                foreach(KeyValuePair<T, long> pair in _two){
                    if(favorite_character.ContainsKey(pair.Key)){
                        favorite_character[pair.Key] += pair.Value;
                    } else {
                        favorite_character.Add(pair.Key, pair.Value);
                    }
                }
                
                return favorite_character;
            }
            #pragma warning restore

            //Okay, well let's just bang this out one at a time
            //number of replies is easy
            RoleplayStatistics statistics = new RoleplayStatistics();

            statistics.number_of_replies = one.number_of_replies + two.number_of_replies;

            //Average reply length can be updated by finding the total reply length of each, adding the two, then dividing that:
            double total_reply_length_one = one.number_of_replies * one.average_reply_length;
            double total_reply_length_two = two.number_of_replies * two.average_reply_length;
            statistics.average_reply_length = (total_reply_length_one + total_reply_length_two)/statistics.number_of_replies;

            //Longest message is just a comparison
            statistics.longest_message_length = Math.Max(one.longest_message_length, two.longest_message_length);

            //We can just add the dictionaries for favorite character
            statistics.favorite_character = AddDictionaries<string>(one.favorite_character, two.favorite_character);

            //Word counter is done in a similar fashion.
            statistics.word_counter = AddDictionaries<string>(one.word_counter, two.word_counter);

            //The average number of unique words is words / amount. So its same idea as total_reply_length
            double unique_words_one = one.number_of_replies * one.average_unique_words;
            double unique_words_two = two.number_of_replies * two.average_unique_words; 
            statistics.average_unique_words = (unique_words_one + unique_words_two)/statistics.number_of_replies;

            //We already have word count
            statistics.number_of_words = statistics.word_counter.Sum(kv => kv.Value);

            //Just combine our lists
            foreach(string s in one.palindromes){
                statistics.palindromes.Add(s);
            }
            foreach(string s in two.palindromes){
                if(!statistics.palindromes.Contains(s)){
                    statistics.palindromes.Add(s);
                }
            }
            
            //We can find our favorite words by just finding our top three
            List<string> favorite = new List<string>();
                IEnumerable<KeyValuePair<string, long>> top_three = statistics.word_counter.OrderByDescending(kv => kv.Value);
                foreach(KeyValuePair<string, long> pair in top_three){
                    if(!PublicArrays.common_words.Contains(pair.Key.ToLower()) && !RoleplayMessageParser.MessageParser.IsCharacter(pair.Key, one.guild_id).Result){
                        favorite.Add(pair.Key);
                    }
                    if(favorite.Count == 3){
                        break;
                    }
                }
            statistics.favorite_words = favorite.ToArray();

            //Alliteration
            statistics.alliteration_counter = AddDictionaries<char>(one.alliteration_counter, two.alliteration_counter);
            long max_char = 0;
            long sum = 0;
            foreach(KeyValuePair<char, long> pair in statistics.alliteration_counter){
                if(pair.Value > max_char){
                    max_char = pair.Value;
                }
                sum += pair.Value;
            }
            statistics.alliteration_score = max_char/(double)sum;

            //Punctuation
            statistics.punctionation_counter = AddDictionaries<char>(one.punctionation_counter, two.punctionation_counter);

            //Word length
            long total_words = statistics.word_counter.Sum(kv => kv.Value);
            long word_count = 0;
            foreach(KeyValuePair<string, long> kv in statistics.word_counter){
                word_count += kv.Key.Length;
            }

            statistics.average_wordlength = word_count/total_words;


            return statistics;
        }

        public Data ExtractData(Data data){
            return data.Extract(this);
        }
    }

    public static class RoleplayMessageParser{

        public class MessageParser{

            public static int instances = 0;
            public int instance = 0;

            public RoleplayStatistics statistics;
            public string message;
            ulong id;

            ulong guild_id;

            public MessageParser(string message, ulong id, ulong guild_id){
                statistics = new RoleplayStatistics();
                this.message = message;
                this.id = id;
                instance = instances;
                instances++;
                this.guild_id = guild_id;

            }

            private Dictionary<char, long> GetAlliterations(string[] words){
                Dictionary<char, long> alliteritions = new Dictionary<char, long>();

                foreach(string s in words){
                    string word = s.ToLower();
                    if(Regex.Match(word,@"^[a-z]").Success){
                        char c = s[0]; //I am going to be naive here
                        try{
                            alliteritions[c]++;
                        }catch{
                            alliteritions.Add(c, 1);
                        }
                    }
                }

                return alliteritions;
            }

            private double GetAlliterationScore(Dictionary<char, long> alliteration){
                long sum = 0;
                long max = 0;
                foreach(KeyValuePair<char, long> pair in alliteration){
                    sum += pair.Value;
                    if(pair.Value > max){
                        max = pair.Value;
                    }
                }
                try{
                    return max/sum;

                }catch{return 0;}
            }

            private string GetAlphaNumericOnly(string input){
                return Regex.Replace(input, @"[^a-zA-Z0-9 ]+", "");
            }

            public Task<Dictionary<string, long>> GetUniqueWords(string[] words){
                Dictionary<string, long> word_counter = new Dictionary<string, long>();

                foreach(string word in words){
                    string final_word = GetAlphaNumericOnly(word);
                    try{
                        word_counter[final_word]++;
                    }catch{
                        word_counter.Add(final_word, 1);
                    }
                }

                return Task.FromResult(word_counter);
            }

            public Task<double> GetAverageWordLength(string[] words){
                long sum = 0;
                foreach(string s in words){
                    sum += GetAlphaNumericOnly(s).Length;
                }
                return Task.FromResult(sum / (double)words.Length);
            }

            public async Task<string[]> GetFavoriteWords(Dictionary<string, long> word_counter){
                List<string> favorite = new List<string>();
                IEnumerable<KeyValuePair<string, long>> top_three = word_counter.OrderByDescending(kv => kv.Value);
                foreach(KeyValuePair<string, long> pair in top_three){
                    if(!PublicArrays.common_words.Contains(pair.Key.ToLower()) && ! await IsCharacter(pair.Key, guild_id)){
                        favorite.Add(pair.Key);
                    }
                    if(favorite.Count == 3){
                        break;
                    }
                }
                return favorite.ToArray();
            }

            public async static Task<bool> IsCharacter(string name, ulong guild_id){
                List<Character> characters = await DatabaseAccessor.LoadItems<Character>(DatabaseAccessor.CHARACTER_PATH(guild_id));
                foreach(Character character in characters){
                    if(character.name.ToLower() == name){
                        return true;
                    }
                }
                return false;
            }

            private string GetPuncuationOnly(string input){
                // Define a string containing all punctuation characters
                string punctuation = "!\"'(),-.:;?";

                // Initialize an empty string to store the punctuation characters
                string result = "";

                // Loop through each character in the input string
                foreach (char c in input){
                    // Check if the character is a punctuation character
                    if (punctuation.Contains(c)){
                        // Append the punctuation character to the result string
                        result += c;
                    }
                }

                return result;
            }

            //Our words should be combined with puncuations
            public Dictionary<char, long> GetPuncuation(string[] words){
                Dictionary<char, long> punctuation = new Dictionary<char, long>();
                foreach(string s in words){
                    foreach(char c in GetPuncuationOnly(s).ToCharArray()){
                        try{
                            punctuation[c]++;
                        } catch{
                            punctuation.Add(c, 1);
                        }
                    }
                }

                return punctuation;
            }

            public async Task<Dictionary<string, long>> GetFavoriteCharacter(string[] words){
                List<string> ignore = new List<string>(new string[]{" they ", " them ", " their ", " theirs ", " theirself ", " he ", " him ", " his ", " himself ", " she ", " her ", " hers ", " herself ", " it ", " its ", " itself "});
                Dictionary<string, long> characters = new Dictionary<string, long>();

                while(true){
                    string name = await GetNameBrute(words, ignore);
                    if(name == ""){
                        return characters;
                    }
                    try{
                        Character character = await CharacterAccessor.GrabCharacter(name, id, guild_id);
                        if(character.user_id == id){
                            characters.Add(name, 1);
                            return characters;
                        }
                    } catch{}finally {
                        ignore.Add(name);
                    }
                }
            }

            public async static Task<string> GetNameBrute(IEnumerable<string> doc_string, IEnumerable<string> ignore){
                Dictionary<string, int> nouns = new Dictionary<string, int>();

                foreach(string s in doc_string){
                    foreach(Match m in Regex.Matches(s, @"\b\b[A-Z][a-z]+\b")){
                        string value = m.Value;
                        if(!nouns.ContainsKey(value)){
                            nouns.Add(value, 1);
                        } else {
                            nouns[value] += 1;
                        }
                    }
                }

                //Now we're going to assume the noun used most is the name. Niave, but it should be true
                //Also, we're going to ignore any and all pronouns
                string noun = "";
                int max = 0;
                foreach(KeyValuePair<string, int> pair in nouns){
                    if(!Regex.Match(pair.Key, string.Join("|", ignore)).Success){
                        if(pair.Value > max){
                            noun = pair.Key;
                            max = pair.Value;
                        }
                    }
                }
                return noun;
            }

            public async Task Start(){
                await Logger.Log($"Thread {instance}: Started message parsing.", guild_id);
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                //parse message
                string[] words = message.Split(" ");

                statistics = new RoleplayStatistics();

                //There's only going to be one reply within this construction
                statistics.number_of_replies = 1;

                statistics.alliteration_counter = GetAlliterations(words);
                statistics.alliteration_score = GetAlliterationScore(statistics.alliteration_counter);

                statistics.word_counter = await GetUniqueWords(words);

                statistics.average_reply_length = words.Length;
                statistics.average_unique_words = statistics.word_counter.Count;
                statistics.average_wordlength = await GetAverageWordLength(words);

                statistics.favorite_character = await GetFavoriteCharacter(words);
                statistics.favorite_words = await GetFavoriteWords(statistics.word_counter);
                statistics.longest_message_length = words.Length;
                statistics.number_of_words = words.Length;
                // statistics.palindromes = I'm ignoring this for nowwww
                statistics.punctionation_counter = GetPuncuation(words);

                statistics.guild_id = guild_id;


                //Fill out statistics struct
                await StatisticsAccessor.SaveStatistics(statistics, id, guild_id);


                stopwatch.Stop();
                await Logger.Log($"Thread {instance}: Finished parsing message. Took ${stopwatch.ElapsedMilliseconds} milliseconds.", guild_id);

            }

        }

        public static async Task StartParsing(string content, ulong id, ulong guild_id){
            //Start thread here
            MessageParser parser = new MessageParser(content, id, guild_id);

            await parser.Start();
            
        }

    }
    }

}