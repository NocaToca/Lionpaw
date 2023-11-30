

using System.Text.RegularExpressions;
using Lionpaw.Queries;

namespace Lionpaw{

    namespace Parsing{

        public static partial class Parser{
            public static async Task<List<QueryResult>> GetQueryResults(List<Query> queries, List<string> text){

                List<Task<QueryResult>> tasks = new List<Task<QueryResult>>();
                foreach(Query query in queries){
                    tasks.Add(GetResult(query, text));
                }

                QueryResult[] results = await Task.WhenAll(tasks); 

                return results.ToList();
            }

            public static Task<QueryResult> GetResult(Query query, List<string> text){

                object? result = null;

                //We'll hand some cases I know how to handle
                switch(query.query_type){
                    case QueryType.NAME:
                        result = GetName(text);
                        break;
                    case QueryType.AGE:
                        result = GetAge(text);
                        break;
                    case QueryType.GENDER:
                        result = GetGender(text);
                        break;
                    case QueryType.OCCUPATION:
                    case QueryType.WARRIOR_CATS_RANK:
                    case QueryType.CLAN:
                    case QueryType.ORIENTATION:
                    case QueryType.CUSTOM:
                        Func<Query, List<string>, string> parse = (query.tokens.Count == 0) ?
                        (q, s) => {return GetNiave(q, s);} :
                        (q, s) => {return GetFromTokens(q, s);};

                        if(query.query_value == QueryValue.NUMBER){
                            result = parse(query, text);
                        } else 
                        if(query.query_value == QueryValue.BOOLEAN){
                            result = parse(query, text);
                        } else {
                            result = parse(query, text);
                        }
                        break;
                }
                if(result == null){
                    throw new Exception();
                }

                return Task<QueryResult>.FromResult(new QueryResult(query, result));

            }

            //If they don't provide tokens, I'm going to have to assume that what we're looking for is right after
            //The query name
            public static string GetNiave(Query query, List<string> all_text){
                for(int i = 0; i < all_text.Count-1; i++){
                    string text = all_text[i];
                    if(text.ToLower() == query.name.ToLower()){
                        return all_text[i+1];
                    }
                }

                return "Not Found";
            }

            public static string GetFromTokens(Query query, List<string> all_text){
                //We're going to do a counter approach
                List<string> tokens = query.tokens;

                long max = 0;
                
                string result = tokens[0];
                //Foreach would be the same thing but this reads clearer for my purpose
                for(int i = 0; i < tokens.Count; i++){
                    int counter = 0;

                    foreach(string text in all_text){
                        if(Regex.Match(text, tokens[i]).Success){
                            counter += 1;
                        }
                    }

                    if(counter > max){
                        result = tokens[i];
                        max = counter;
                    }
                }

                return result;
            }

            public static string GetName(IEnumerable<string> doc_string){
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
                string[] ignore = {" they ", " them ", " their ", " theirs ", " theirself ", " he ", " him ", " his ", " himself ", " she ", " her ", " hers ", " herself ", " it ", " its ", " itself "};
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

            public static int? GetAge(List<string> doc_string){
                for(int i = 0; i < doc_string.Count; i++){
                    string s = doc_string[i];
                    if(s.ToLower() == "age"){
                        try{
                            return int.Parse(doc_string[i+1].Split(" ")[0]);
                        #pragma warning disable 0168
                        } catch(Exception e){
                            int r = 0;
                            while(!int.TryParse(doc_string[i++].Split(" ")[0], out r));
                            return r;
                        }
                        #pragma warning restore
                    }
                }
                return null;
            }

            public static string GetGender(List<string> doc_string){
                const int MASC = 0;
                const int FEM = 1;
                const int NB = 2;

                int[] acc = new int[3];

                
                string[] masc_pronouns = {" he ", " him ", " his ", " himself "};
                string[] fem_pronouns = {" she ", " her ", " hers ", " herself "};
                string[] nb_pronouns = {" they ", " them ", " their ", " theirs ", " theirself "};

                foreach(string s in doc_string){
                    acc[MASC] += Regex.Matches(s, string.Join("|", masc_pronouns), RegexOptions.IgnoreCase).Count;
                    acc[FEM] += Regex.Matches(s, string.Join("|", fem_pronouns), RegexOptions.IgnoreCase).Count;
                    acc[NB] += Regex.Matches(s, string.Join("|", nb_pronouns), RegexOptions.IgnoreCase).Count;
                }
                // Console.WriteLine(acc[MASC]);
                // Console.WriteLine(acc[FEM]);
                // Console.WriteLine(acc[NB]);

                if(acc[MASC] > acc[FEM] && acc[MASC] > acc[NB]){
                    return "male";
                } else
                if(acc[FEM] > acc[MASC] && acc[FEM] > acc[NB]){
                    return "female";
                } else {
                    return "non-binary";
                }
            }


        }
        

    }
}