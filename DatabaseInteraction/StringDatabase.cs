using Newtonsoft.Json;

//Basically acts as a stored dictionary for our messages
public static class StringDatabase{

    public class StringKey{
        [JsonProperty("key")]
        public string key;

        [JsonProperty("value")]
        public string value;

        [JsonConstructor]
        public StringKey(string key, string value){
            this.key = key;
            this.value = value;
        }

    }

    public static void SaveString(string key, string value){
        string file_path = "Database/cats.json";

        // Create a list to hold all cats
        List<StringKey>? string_keys = new List<StringKey>();
        StringKey new_key = new StringKey(key, value);

        // Check if the file exists
        if (File.Exists(file_path)){
            // If the file exists, load the existing data
            string json_data = File.ReadAllText(file_path);
            string_keys = JsonConvert.DeserializeObject<List<StringKey>>(json_data);
            if(string_keys == null){
                string_keys = new List<StringKey>();
            }

            StringKey? existing_key = string_keys.FirstOrDefault(c => key == c.key);
            if(existing_key != null){
                existing_key = new_key;
                // Serialize the list back to JSON
                string _updated_json_data = JsonConvert.SerializeObject(string_keys);

                // Write the updated JSON data back to the file
                File.WriteAllText(file_path, _updated_json_data);
            }
        }

        // Add the new cat to the list
        string_keys.Add(new_key);

        // Serialize the list back to JSON
        string updated_json_data = JsonConvert.SerializeObject(string_keys);

        // Write the updated JSON data back to the file
        File.WriteAllText(file_path, updated_json_data);


    }

    public static Dictionary<string, string> GetDatabase(){
        string file_path = "Database/strings.json";

        // Check if the file exists
        if (File.Exists(file_path)){
            // If the file exists, load the existing data
            string json_data = File.ReadAllText(file_path);
            List<StringKey>? strings = JsonConvert.DeserializeObject<List<StringKey>>(json_data);

            if(strings == null){
                throw new Exception("No cats to load");
            }

            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach(StringKey string_key in strings){
                dictionary.Add(string_key.key, string_key.value);
            }

            return dictionary;
        }

        throw new Exception("No cats to load");
    }

    public static string LoadString(string key){
        return GetDatabase()[key];
    }


}