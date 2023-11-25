using Newtonsoft.Json;

public static class DatabaseReader{

    //Just want to save the cat into our database
    public static Cat LoadCat(string name){
        // Define the file path
        string file_path = "Database/cats.json";

        lock(DatabaseLockObject.Lock_Object){
            // Check if the file exists
            if (File.Exists(file_path)){
                // If the file exists, load the existing data
                string json_data = File.ReadAllText(file_path);
                List<Cat>? cats = JsonConvert.DeserializeObject<List<Cat>>(json_data);

                if(cats == null){
                    throw new Exception("No cats in database to load");
                }

                // Search for the cat with the matching name
                Cat? cat = cats.Find(c => c.GetName().ToLower() == name.ToLower());

                if(cat == null){
                    throw new Exception("Cat not found");         
                }

                return cat;
            }

        }
        

        // If the file does not exist or there is no matching cat, return null
        throw new Exception("Cat not found");
    }

    public static List<Cat> LoadCats(){
        string file_path = "Database/cats.json";
        
        lock(DatabaseLockObject.Lock_Object){

            // Check if the file exists
            if (File.Exists(file_path)){
                // If the file exists, load the existing data
                string json_data = File.ReadAllText(file_path);
                List<Cat>? cats = JsonConvert.DeserializeObject<List<Cat>>(json_data);

                if(cats == null){
                    throw new Exception("No cats to load");
                }

                return cats;
            }

        }
        

        throw new Exception("No cats to load");
    }


}