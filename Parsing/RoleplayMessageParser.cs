

using System.Text.RegularExpressions;

namespace Roleplay{

    //Statistic ideas:
    /*
        Number of replies!
        Average wordlength
        Average reply length
        Top 3 favorite words!
        Longest message
        Message Length Distribution (Gives them a fun little chart of their message lengths)
        Alliteration Score (Their most used starting letter and on average how much they use it)
        Palindrome counts! (How many palindromes they've made)
        Punctuation Counter (How many times they use . , ? ! ; and :)
        Vocabulary Robustness (How many different words do you use?)
        Average Unique words per message
        Most used Character!
    
    */

    //This is going to be a base class to hold the statistics for a single message.
    //We'll also do this in a seperate thread so my computer doesn't stall on this computation
    //Important to keep in note we're not saving this class directly, we're going to split the class
    //up into smaller classes to save. This way I can seperate the data easier (I hate cluttered data!)
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

        public RoleplayStatistics(){
        }

        public RoleplayStatistics(FreeData? free_data, FavoriteCharacter? favorite_character, WordStorage? word_data, AlliterationData? alliteration_data, PuncuationData? puncuation_data){
            if(free_data != null){
                //Free Data
                this.number_of_replies = free_data.number_of_replies;
                this.average_reply_length = free_data.average_reply_length;
                this.longest_message_length = free_data.longest_message;
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
            IEnumerable<KeyValuePair<string, long>> top_three = statistics.word_counter.OrderByDescending(kv => kv.Value).Take(3);
            foreach(KeyValuePair<string, long> pair in top_three){
                favorite.Add(pair.Key);
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
    }

    public static class RoleplayMessageParser{

        public static List<Thread> threads;

        public class MessageParser{

            public RoleplayStatistics statistics;
            public string message;
            ulong id;

            public MessageParser(string message, ulong id){
                statistics = new RoleplayStatistics();
                this.message = message;
                this.id = id;

            }

            private Dictionary<char, long> GetAlliterations(string[] words){
                Dictionary<char, long> alliteritions = new Dictionary<char, long>();

                foreach(string s in words){
                    string word = s.ToLower();
                    if(Regex.Match(word,@"^[a-z]").Success){
                        char c = s[0]; //I am going to be naive here
                        try{
                            alliteritions[c]++;
                        }catch(Exception e){
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
                    if(pair.Value > max){
                        max = pair.Value;
                    }
                    sum += pair.Value;
                }

                return max/sum;
            }

            private string GetAlphaNumericOnly(string input){
                return Regex.Replace(input, @"[^a-zA-Z0-9 ]+", "");
            }

            public Dictionary<string, long> GetUniqueWords(string[] words){
                Dictionary<string, long> word_counter = new Dictionary<string, long>();

                foreach(string word in words){
                    string final_word = GetAlphaNumericOnly(word);
                    try{
                        word_counter[final_word]++;
                    }catch(Exception e){
                        word_counter.Add(final_word, 1);
                    }
                }

                return word_counter;
            }

            public double GetAverageWordLength(string[] words){
                long sum = 0;
                foreach(string s in words){
                    sum += GetAlphaNumericOnly(s).Length;
                }
                return sum / words.Length;
            }

            public string[] GetFavoriteWords(Dictionary<string, long> word_counter){
                List<string> favorite = new List<string>();
                IEnumerable<KeyValuePair<string, long>> top_three = word_counter.OrderByDescending(kv => kv.Value).Take(3);
                foreach(KeyValuePair<string, long> pair in top_three){
                    favorite.Add(pair.Key);
                }
                return favorite.ToArray();
            }

            public void Start(){
                //parse message
                string[] words = message.Split(" ");

                statistics = new RoleplayStatistics();

                //There's only going to be one reply within this construction
                statistics.number_of_replies = 1;

                statistics.alliteration_counter = GetAlliterations(words);
                statistics.alliteration_score = GetAlliterationScore(statistics.alliteration_counter);

                statistics.word_counter = GetUniqueWords(words);

                statistics.average_reply_length = words.Length;
                statistics.average_unique_words = statistics.word_counter.Count;
                statistics.average_wordlength = GetAverageWordLength(words);

                //statistics.favorite_character = I'm ignoring this for now
                statistics.favorite_words = GetFavoriteWords(statistics.word_counter);
                statistics.longest_message_length = words.Length;
                statistics.number_of_words = words.Length;
                // statistics.palindromes = I'm ignoring this for nowwww
                // statistics.punctionation_counter = Same for this


                //Fill out statistics struct

                //Save to database
                lock(RoleplayDatabase.lock_object){
                    RoleplayDatabase.Save(statistics, id);
                }
            }

        }

        public static void StartParsing(string content, ulong id){
            //Start thread here
            MessageParser parser = new MessageParser(content, id);

            Thread parsing_thread = new Thread(parser.Start);
            parsing_thread.Start();
            
        }

    }

    

}
