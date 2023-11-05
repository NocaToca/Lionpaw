using Newtonsoft.Json;

public static class DatabaseWriter{

    //Just want to save the cat into our database
    public static bool SaveCat(Cat cat){
        // Define the file path
        string file_path = "Database/cats.json";

        // Create a list to hold all cats
        List<Cat>? cats = new List<Cat>();

        // Check if the file exists
        if (File.Exists(file_path)){
            // If the file exists, load the existing data
            string json_data = File.ReadAllText(file_path);
            cats = JsonConvert.DeserializeObject<List<Cat>>(json_data);
            if(cats == null){
                cats = new List<Cat>();
            }

            Cat? existing_cat = cats.FirstOrDefault(c => c.GetName() == cat.GetName());
            if(existing_cat != null){
                existing_cat = cat;
                // Serialize the list back to JSON
                string _updated_json_data = JsonConvert.SerializeObject(cats);

                // Write the updated JSON data back to the file
                File.WriteAllText(file_path, _updated_json_data);

                return true;
            }
        }

        // Add the new cat to the list
        cats.Add(cat);

        // Serialize the list back to JSON
        string updated_json_data = JsonConvert.SerializeObject(cats);

        // Write the updated JSON data back to the file
        File.WriteAllText(file_path, updated_json_data);

        return true;
    }

    public static bool RemoveCat(string name){
        // Define the file path
        string file_path = "Database/cats.json";

        // Create a list to hold all cats
        List<Cat>? cats = new List<Cat>();

        // Check if the file exists
        if (File.Exists(file_path)){

            // If the file exists, load the existing data
            string json_data = File.ReadAllText(file_path);
            cats = JsonConvert.DeserializeObject<List<Cat>>(json_data);

            if (cats == null){
                cats = new List<Cat>();
            }

            // Find and remove the cat with the specified name
            Cat? catToRemove = cats.FirstOrDefault(c => c.GetName().ToLower() == name.ToLower());

            if (catToRemove != null){
                cats.Remove(catToRemove);

                // Serialize the list back to JSON
                string updated_json_data = JsonConvert.SerializeObject(cats);

                // Write the updated JSON data back to the file
                File.WriteAllText(file_path, updated_json_data);

                return true;
            }
        }

        return false; // Cat with specified name not found
    }

    public static bool UpdateCat(Cat updated_cat){
        // Define the file path
        string file_path = "Database/cats.json";

        // Check if the file exists
        if (File.Exists(file_path))
        {
            // If the file exists, load the existing data
            string json_data = File.ReadAllText(file_path);
            List<Cat>? cats = JsonConvert.DeserializeObject<List<Cat>>(json_data);

            if(cats == null){
                throw new Exception("Database empty. No cat to update.");
            }

            // Find the cat with the same name as the updated cat
            Cat? existing_cat = cats.Find(c => c.GetName() == updated_cat.GetName());

            if (existing_cat != null){
                // Update the cat's properties
                existing_cat.EditAge(updated_cat.GetAge());
                existing_cat.EditGender(updated_cat.GetGender());
                existing_cat.EditRank(updated_cat.GetRank());
                existing_cat.EditClan(updated_cat.GetClan());
                existing_cat.EditLink(updated_cat.GetLink());

                // Serialize the updated list back to JSON
                string updated_json_data = JsonConvert.SerializeObject(cats);

                // Write the updated JSON data back to the file
                File.WriteAllText(file_path, updated_json_data);

                return true;
            }
        }

        // If the file does not exist or there is no matching cat, return false
        return false;
    }


}