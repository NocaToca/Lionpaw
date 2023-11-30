
//This basically just handles what we search for in docs. I made
//some support for predefined queries from servers I know
using System.DirectoryServices.Protocols;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace Lionpaw{

    namespace Queries{

        public enum QueryValue{
            [ChoiceName("Number")]
            NUMBER,
            [ChoiceName("Word")]
            WORD,
            BOOLEAN
        }

        public enum QueryType{
            [ChoiceName("Name")]
            NAME,
            [ChoiceName("Age")]
            AGE,
            [ChoiceName("Gender")]
            GENDER,
            [ChoiceName("Clan")]
            CLAN,
            [ChoiceName("Rank (Warrior Cats)")]
            WARRIOR_CATS_RANK,
            [ChoiceName("Orientation")]
            ORIENTATION,
            [ChoiceName("Occupation")]
            OCCUPATION,
            [ChoiceName("Custom")]
            CUSTOM
        }

        public class Query{

            [JsonProperty("name")]
            public string name;

            [JsonProperty("description")]
            public string description;

            [JsonProperty("tokens")]
            public List<string> tokens; //These are used if we have a specific set that the query has to be
            //This is useful for things like gender, ranks, and clans

            [JsonProperty("query_type")]
            public QueryType query_type; //If you don't have a reason to use a custom query type, it's better
            //to just use supported ones so Lionpaw can find it much easier

            [JsonProperty("query_vale")]
            public QueryValue query_value;

            [JsonConstructor]
            public Query(string name, string description, List<string> tokens, QueryType query_type, QueryValue query_value){
                this.name = name;
                this.description = description;
                this.tokens = tokens;
                this.query_type = query_type;
                this.query_value = query_value;
            }

            public Query(QueryType query_type, List<string>? tokens = null){
                if(query_type == QueryType.CUSTOM){
                    throw new Exception();
                }

                this.query_type =query_type;

                if(tokens == null){
                    this.tokens = new List<string>();
                } else
                if(tokens.Count != 0){
                    this.tokens = tokens;
                } else {
                    this.tokens = query_type.QueryTokens();
                }

                name = query_type.QueryName();
                description = query_type.QueryDescription();
                query_value = query_type.GetQueryValue();
            }
        }

        public class QueryResult{

            [JsonProperty("query")]
            public Query query; //The query made from the result

            [JsonProperty("result")]
            public object result;

            [JsonConstructor]
            public QueryResult(Query query, object result){
                this.query = query;
                this.result = result;
            }

        }

        public static class QueryTypeExtender{

            public static string QueryName(this QueryType type){
                switch (type){
                    case QueryType.NAME:
                        return "name";
                    case QueryType.AGE:
                        return "age";
                    case QueryType.GENDER:
                        return "gender";
                    case QueryType.CLAN:
                        return "clan";
                    case QueryType.WARRIOR_CATS_RANK:
                        return "rank";
                    case QueryType.OCCUPATION:
                        return "occupation";
                    case QueryType.ORIENTATION:
                        return "orientation";
                    case QueryType.CUSTOM:
                        throw new InvalidOperationException("CUSTOM is not a valid query type.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            
            public static string QueryDescription(this QueryType type){
                switch (type){
                    case QueryType.NAME:
                        return "The name of the character.";
                    case QueryType.AGE:
                        return "The age of the character.";
                    case QueryType.GENDER:
                        return "The gender of the character";
                    case QueryType.CLAN:
                        return "The clan of the character.";
                    case QueryType.WARRIOR_CATS_RANK:
                        return "The rank of the character.";
                    case QueryType.OCCUPATION:
                        return "The job of the character";
                    case QueryType.ORIENTATION:
                        return "The sexual orientation of the character.";
                    case QueryType.CUSTOM:
                        throw new InvalidOperationException("CUSTOM is not a valid query type.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            public static List<string> QueryTokens(this QueryType type){
                switch (type){
                    case QueryType.NAME:
                        return new List<string>();
                    case QueryType.AGE:
                        return new List<string>();
                    case QueryType.GENDER:
                        return new List<string>();
                    case QueryType.CLAN:
                        return new List<string>();
                    case QueryType.WARRIOR_CATS_RANK:
                        return new List<string>(){"warrior", "apprentice", "kit", "healer", "healer apprentice", "deputy", "leader", "rogue", "loner", "kittypet"};
                    case QueryType.OCCUPATION:
                        return new List<string>();
                    case QueryType.ORIENTATION:
                        return new List<string>(){"heterosexual", "straight", "bisexual", "bi", "heteroflexible", "pansexual", "demisexual", 
                        "gay", "homosexual", "lesbian", "sapphic", "asexual", "allosexaul", "hypersexual"};
                    case QueryType.CUSTOM:
                        throw new InvalidOperationException("CUSTOM is not a valid query type.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            public static QueryValue GetQueryValue(this QueryType type){
                switch (type){
                    case QueryType.NAME:
                        return QueryValue.WORD;
                    case QueryType.AGE:
                        return QueryValue.NUMBER;
                    case QueryType.GENDER:
                        return QueryValue.WORD;
                    case QueryType.CLAN:
                        return QueryValue.WORD;
                    case QueryType.WARRIOR_CATS_RANK:
                        return QueryValue.WORD;
                    case QueryType.OCCUPATION:
                        return QueryValue.WORD;
                    case QueryType.ORIENTATION:
                        return QueryValue.WORD;
                    case QueryType.CUSTOM:
                        throw new InvalidOperationException("CUSTOM is not a valid query type.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

        }

    }

}