using DSharpPlus.SlashCommands;
public enum CatParams{

    [ChoiceName("Name")]
    Name,
    [ChoiceName("Age")]
    Age,
    [ChoiceName("Gender")]
    Gender,
    [ChoiceName("Rank")]
    Rank,
    [ChoiceName("Clan")]
    Clan,
    [ChoiceName("Link")]
    Link,
    None

}

public enum Gender{
    [ChoiceName("Male (He/Him)")]
    male_one,
    [ChoiceName("Male (He/They)")]
    male_two,
    [ChoiceName("Female (She/Her)")]
    female_one,
    [ChoiceName("Female (She/They)")]
    female_two,
    [ChoiceName("Non-binary (They/Them)")]
    non_binary,
    [ChoiceName("Non-Binary (It/Its)")]
    non_binary_two,
    [ChoiceName("Femme Aligned (She/Its)")]
    female_three,
    [ChoiceName("Other! :3")]
    other,
    None
}

public enum Rank{
    [ChoiceName("Leader")]
    Leader,
    [ChoiceName("Deputy")]
    Deputy,
    [ChoiceName("Messenger")]
    Messenger,
    [ChoiceName("Messenger Apprentice")]
    Messenger_App,
    [ChoiceName("Healer")]
    Medic,
    [ChoiceName("Healer Apprentice")]
    Medic_App,
    [ChoiceName("Warrior")]
    Warrior,
    [ChoiceName("Apprentice")]
    Apprentice,
    [ChoiceName("Kit")]
    Kit,
    None,
    [ChoiceName("Elder")]
    Elder

}

public enum Clan{
    [ChoiceName("Cats of the Endless Woodlands")]
    Woodlands,
    [ChoiceName("Cats of the Sandswept Praire")]
    Praire,
    [ChoiceName("Cats of the Thickening Mist")]
    Mist,
    [ChoiceName("Rogue")]
    Rogue,
    [ChoiceName("Loner")]
    Loner,
    [ChoiceName("Kittypet")]
    Kittypet,
    None
}
