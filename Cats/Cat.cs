using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;

public class Cat{
    [JsonProperty("name")]
    string name {get; set;}

    [JsonProperty("age")]
    int? age {get; set;}

    [JsonProperty("gender")]
    Gender? gender {get; set;}

    [JsonProperty("rank")]
    Rank? rank {get; set;}

    [JsonProperty("clan")]
    Clan? clan {get; set;}

    [JsonProperty("link")]
    string link {get; set;}

    [JsonProperty("user")]
    public ulong user {get; private set;}

    [JsonProperty("image")]
    public string image_file;

    [JsonProperty("avaible_for_rp")]
    public bool avaible_for_rp;

    public static readonly string[] cat_speak = new string[]{
        "Mrrrrow!",
        "Meoow!",
        "Mreow!",
        "Meow!",
        "Mrrrow!",
        "Meoooow!",
        "Meeow!",
        "Mrrrrow?",
        "Meoow?",
        "Mreow!",
        "Meow!",
        "Mrrrow?",
        "Meoooow!",
        "Meeow!"
    };

    //Depricated in terms of a json constructor. Kept to update
    // [JsonConstructor]
    public Cat(string name, int? age1, Gender? gender, Rank? rank1, Clan? clan1, string link, ulong user, string image_file)
    {
        this.name = name;
        this.age = age1;
        this.gender = gender;
        this.rank = rank1;
        this.clan = clan1;
        this.link = link;
        this.user = user;
        this.image_file = image_file;
        avaible_for_rp = true;
    }

    [JsonConstructor]
    public Cat(string name, int? age1, Gender? gender, Rank? rank1, Clan? clan1, string link, ulong user, string image_file, bool avaible_for_rp)
    {
        this.name = name;
        this.age = age1;
        this.gender = gender;
        this.rank = rank1;
        this.clan = clan1;
        this.link = link;
        this.user = user;
        this.image_file = image_file;
        this.avaible_for_rp = avaible_for_rp;
    }
    

    public string GetName(){
        return name;
    }
    public void EditName(string name){
        this.name = name;
    }

    public int? GetAge(){
        return age;

    } 
    public void EditAge(int? age){
        this.age = age;
    }

    public Gender? GetGender(){
        return gender;
    }
    public void EditGender(Gender? gender){
        this.gender = gender;
    }

    public Rank? GetRank(){
        return rank;
    }
    public void EditRank(Rank? rank){
        this.rank = rank;
    }

    public Clan? GetClan(){
        return clan;
    }
    public void EditClan(Clan? clan){
        this.clan = clan;
    }

    public string GetLink(){
        return link;
    }
    public void EditLink(string link){
        this.link = link;
    }

    public void EditParam(CatParams param, string value){
        if(param == CatParams.Name){
            EditName(value);
        }
        if(param == CatParams.Age){
            EditAge(int.Parse(value));
        }
        if(param == CatParams.Gender){
            string gender_value = value.ToLower();
            if(gender_value == "female"){
                EditGender(Gender.female_one);
            }
            if(gender_value == "male"){
                EditGender(Gender.male_one);
            }
        }
    }

    public string GetGenderString(){
        switch(gender){
            case Gender.male_one:
                return "Male (He/Him)";
            case Gender.male_two:
                return "Male (He/They)";
            case Gender.female_one:
                return "Female (She/Her)";
            case Gender.female_two:
                return "Female (She/They)";
            case Gender.non_binary:
                return "Non-binary (They/Them)";
            case Gender.non_binary_two:
                return "Non-binary (It/Its)";
            case Gender.female_three:
                return "Femme Aligned (She/It)";
            default:
                return "Other (Please ask!)";

        }
    }

    public static string GetClanString(Clan? clan){
        switch(clan){
            case Clan.Woodlands:
                return "Cats of the Endless Woodlands";
            case Clan.Praire:
                return "Cats of the Sandswept Prairie";
            case Clan.Mist:
                return "Cats of the Thickening Mist";
            case Clan.Rogue:
                return "Rogue";
            case Clan.Loner:
                return "Loner";
            case Clan.Kittypet:
                return "Kittypet";
            case Clan.None:
                return "None";
            default:
                return "None"; // Handle default case if needed
        }
    }

    public static string GetRankString(Rank? rank){
        switch (rank){
            case Rank.Leader:
                return "Leader";
            case Rank.Deputy:
                return "Deputy";
            case Rank.Messenger:
                return "Messenger";
            case Rank.Messenger_App:
                return "Messenger Apprentice";
            case Rank.Medic:
                return "Healer";
            case Rank.Medic_App:
                return "Healer Apprentice";
            case Rank.Warrior:
                return "Warrior";
            case Rank.Apprentice:
                return "Apprentice";
            case Rank.Kit:
                return "Kit";
            case Rank.Elder:
                return "Elder";
            case Rank.None:
                return "None";
            default:
                return "None"; // Handle default case if needed
        }
    }

    public string GetPronouns(){
        switch(gender){
            case Gender.male_one:
                return "He/Him";
            case Gender.male_two:
                return "He/They";
            case Gender.female_one:
                return "She/Her";
            case Gender.female_two:
                return "She/They";
            case Gender.non_binary:
                return "They/Them";
            case Gender.female_three:
                return "She/It";
            default:
                return "";
        }
    }

    public int AgeUp(int time){
        if(age != null){
            age += time;
            return (int)age;
        }
        return -1;
    }

    public string GetCondensedString(){
        //Requires fields to be non-null
        return "**" + name + "(" + GetPronouns() + ")" + ":** " + GetClanString(clan) + " | " + GetRankString(rank) +"\n";
    }

    public DiscordEmbed BuildEmbed(DiscordGuild? guild){

        string title = "**✧⋄⋆⋅⋆⋄" + this.name + "⋄⋆⋅⋆⋄✧**";

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
            Title = title
        };
        if(age != null){
            embed.AddField("__AGE__", $"{age} Moons. ({((double)age/12.0):F2} years) \n");
        } else {
            embed.AddField("__AGE__", "N/A");
        }
        if(gender != null){
            embed.AddField("__GENDER__", GetGenderString() + "\n");
        } else {
            embed.AddField("__GENDER__", "N/A");
        }

        if(clan != null){
            embed.AddField("__CLAN__", GetClanString((Clan)clan) + "\n" );
        } else {
            embed.AddField("__CLAN__", "N/A" );
        }

        if(rank != null){
            embed.AddField("__RANK__", GetRankString((Rank)rank) + "\n");
        } else {
            embed.AddField("__RANK__", "N/A");
        }

        embed.WithColor(DiscordUtils.GetColorFromClan(clan));
        string name;

        //After careful consideration, compiler is dumb because the
        //null reference is caught
        #pragma warning disable 8602
        #pragma warning disable 0168
        try{ 
            name = guild.GetMemberAsync(user).Result.DisplayName;
        }catch(Exception e){
            name = user.ToString();
        }
        #pragma warning restore
         
        embed.WithFooter($"Belongs to {name}.");

        return embed.Build();
    }

    
}