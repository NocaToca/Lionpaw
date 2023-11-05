using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class CatParser{

    public static string? GetName(List<string> doc_string){
        for(int i = 0; i < doc_string.Count; i++){
            string s = doc_string[i];
            if(s.ToLower() == "name"){
                string r_string = doc_string[i+1];
                return r_string;
            }
        }
        return null;
    }

    //There are specific things we know about names that can help us
    //The first big thing is that they are proper nouns - that means that they are always capitialized
    //Let's grab all capital words. We will grab the start of sentences, but names also commonly start sentences
    //Let's also count each time a noun appears
    public static string GetNameBrute(List<string> doc_string){
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
        string noun = "";
        int max = 0;
        foreach(KeyValuePair<string, int> pair in nouns){
            if(pair.Value > max){
                noun = pair.Key;
                max = pair.Value;
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

    public static Gender? GetGender(List<string> doc_string){
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
            return Gender.male_one;
        } else
        if(acc[FEM] > acc[MASC] && acc[FEM] > acc[NB]){
            return Gender.female_one;
        } else {
            return Gender.non_binary;
        }

    }

    private static Rank GetRankFromString(string s){
        switch(s.ToLower()){
            case "leader":
                return Rank.Leader;
            case "deputy":
                return Rank.Deputy;
            case "messenger":
                return Rank.Messenger;
            case "messenger apprentice":
                return Rank.Messenger_App;
            case "medicine cat":
                return Rank.Medic;
            case "medicine cat apprentice":
                return Rank.Medic_App;
            case "warrior":
                return Rank.Warrior;
            case "apprentice":
                return Rank.Apprentice;
            case "kit":
                return Rank.Kit;
            default:
                return Rank.None;
        }
    }

    public static Rank? GetRank(List<string> doc_string){
        for(int i = 0; i < doc_string.Count; i++){
            string s = doc_string[i];
            if(s.ToLower() == "ranking"){
                string r_string = doc_string[i+1];
                return GetRankFromString(r_string);
            }
        }
        return null;
    }

    //There are a few things we can do in order to brute force the rank
    //One of the easiest things is to look for the words "paw" and "kit" and see how often they appear
    //Now, we can't immediately assume that - so we do have to see if they are a warrior first.
    //We'll want to grab a name. We're going to pass that in. Should be found from bruteforce name 
    public static Rank GetRankBrute(List<string> doc_string, string name){
        //Honestly, assuming the name is correct this should be easy.
        //Let's just look to see if there is a paw or not
        if(Regex.Matches(name, @"paw", RegexOptions.IgnoreCase).Count > 0){
            return Rank.Apprentice;
        } else
        if(Regex.Matches(name, @"kit", RegexOptions.IgnoreCase).Count > 0){
            return Rank.Kit;
        } else {
            return Rank.Warrior;
        }

    }

    private static Clan GetClanFromString(string s){
        switch (s.ToLower()){
            case "cats of the endless woodlands":
            case "woodlands":
            case "endless woodlands":
                return Clan.Woodlands;
            case "praire":
            case "sandswept praire":
            case "cats of the sandswept praire":
            case "cats spanning the sandswept praire":
                return Clan.Praire;
            case "mist":
            case "thickening mist":
            case "cats of the thickening mist":
            case "cats among the thickening mist":
                return Clan.Mist;
            case "rogue":
                return Clan.Rogue;
            case "loner":
                return Clan.Loner;
            case "kittypet":
                return Clan.Kittypet;
            default:
                return Clan.None;
        }
    }

    public static Clan? GetClan(List<string> doc_string){
        for(int i = 0; i < doc_string.Count; i++){
            string s = doc_string[i];
            if(s.ToLower() == "allegiance"){
                string r_string = doc_string[i+1];
                return GetClanFromString(r_string);
            }
        }
        return null;
    }

}