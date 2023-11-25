using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Text.RegularExpressions;
using System.Diagnostics;

public class CatCommands : ApplicationCommandModule {

    [SlashCommandPermissions(Permissions.Administrator)]
    [SlashCommand("addcat", "Adds the specific cat to the database.")]
    public async Task AddCat(InteractionContext ctx,
    [Option("UserID", "The ID of the user of who this cat belongs to")] DiscordUser user,
    [Option("Link", "The link to the doc of the cat.")] string link,
    [Option("Name", "Cat name")] string name = "",
    [Option("Age", "Cat age")] long age = -1,
    [Option("Gender", "The gender of the cat!")] Gender gender = Gender.other,
    [Option("Rank", "The rank of the cat.")] Rank rank = Rank.None,
    [Option("Clan", "The clan of the cat.")] Clan clan = Clan.None,
    [Option("AutoFindMember", "Use false if adding somewhere else besides a submissions channel.")] bool auto_find_member = true){

        Status status = Status.ERROR;
        if(ctx.Channel.GuildId == null){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
            return;
        }

        Stopwatch watch = new Stopwatch();
        watch.Start();

        try{
            bool docs = Regex.IsMatch(link, @"document", RegexOptions.IgnoreCase);
            bool slides = Regex.IsMatch(link, @"presentation", RegexOptions.IgnoreCase);

            List<string> pulled_text = new List<string>();
            if(docs){
                pulled_text = DocsHandler.GetAllText(DocsHandler.GetDocIdFromURL(link));
                pulled_text = DocsHandler.FormatTextForParsing(pulled_text);    
            } else
            if(slides){
                pulled_text = SlidesHandler.GetAllText(SlidesHandler.GetPresentationIdFromURL(link));
                pulled_text = DocsHandler.FormatTextForParsing(pulled_text);
            }
            
            
            string? cat_name = name;
            if(name == ""){
                cat_name = CatParser.GetName(pulled_text);
                if(cat_name == null){
                    cat_name = CatParser.GetNameBrute(pulled_text);
                }
            }
            

            int? cat_age = (int)age;
            if(age == -1){
                cat_age = CatParser.GetAge(pulled_text);
            }

            Gender? cat_gender = gender;
            if(gender == Gender.other){
                cat_gender = CatParser.GetGender(pulled_text);
            }
            
            Rank? cat_rank = rank;
            if(rank == Rank.None){
                cat_rank = CatParser.GetRank(pulled_text);
            }
            if(cat_rank == null){
                cat_rank = CatParser.GetRankBrute(pulled_text, cat_name);
            }

            Clan? cat_clan = clan;
            if(clan == Clan.None){
                cat_clan = CatParser.GetClan(pulled_text);
            }

            ulong user_id = user.Id;

            if(cat_name == null){
                throw new Exception("Did not add cat. No name found.");
            }
            
            Cat cat = new Cat(cat_name, cat_age, cat_gender, cat_rank, cat_clan, link, user_id, "none");

            DatabaseWriter.SaveCat(cat);
            await Bot.bot.OnCatAdded(cat, ctx);

            await Bot.client.GetSlashCommands().RefreshCommands();
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Added cat " + name + "!").AddEmbed(cat.BuildEmbed(ctx.Channel.Guild)));
            status = Status.SUCCESS;
        } catch (Exception e){
            Cat cat = new Cat("Temp", 0, Gender.None, Rank.None, Clan.None, link, user.Id, "none");
            DatabaseWriter.SaveCat(cat);
            await Bot.client.GetSlashCommands().RefreshCommands();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Added cat but couldn't parse: " + e.Message).AddEmbed(cat.BuildEmbed(ctx.Channel.Guild)));
            status = Status.ERROR;
        }
        watch.Stop();
        Logger.LogCommand("addcat", ctx, status, watch);
        
    } 

    [SlashCommandPermissions(Permissions.Administrator)]
    [SlashCommand("editcat", "Edits the specified cat with the updated values!")]
    public async Task EditCat(InteractionContext ctx, 
    [ChoiceProvider(typeof(CatChoiceProvider))]
    [Option("Name", "The name of the cat")] string name,
    [Option("Link", "The link to the doc of the cat.")] string link = "",
    [Option("ChangingName", "New cat name")] string new_name = "",
    [Option("Age", "Cat age")] long age = -1,
    [Option("Rank", "The rank of the cat.")] Rank rank = Rank.None,
    [Option("Clan", "The clan of the cat.")] Clan clan = Clan.None,
    [Option("Gender", "Gender to change to.")] Gender gender = Gender.None){
        Status status = Status.ERROR;
        Stopwatch watch = new Stopwatch();
        watch.Start();

        Cat cat = DatabaseReader.LoadCat(name);
        await Bot.bot.OnCatRemoved(cat,ctx);
        DatabaseWriter.RemoveCat(name);

        if(ctx.Channel.GuildId == null){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
            return;
        }

        try{
            if(cat.user != ctx.User.Id){
                Permissions permissions = ctx.Guild.GetMemberAsync(ctx.User.Id).Result.Permissions;
                if(!permissions.HasPermission(Permissions.Administrator)){
                    throw new InvalidOperationException("You cannot edit cats that are not yours!");
                }
            }

        }catch(InvalidOperationException e){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(e.Message));
            return;
        }

        if(new_name != ""){
            cat.EditName(new_name);
        }

        if(age != -1){
            cat.EditAge((int)age);
        }

        if(rank != Rank.None){
            cat.EditRank(rank);
        }

        if(clan != Clan.None){
            cat.EditClan(clan);
        }

        if(link != ""){
            cat.EditLink(link);
        }

        if(gender != Gender.None){
            cat.EditGender(gender);
        }

        DatabaseWriter.SaveCat(cat);
        await Bot.bot.OnCatAdded(cat, ctx);

        await Bot.client.GetSlashCommands().RefreshCommands();
        // CatChoiceProvider.RefreshDatabase();

        status = Status.SUCCESS;

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Updated cat " + name + "!"));

        watch.Stop();
        Logger.LogCommand("editcat", ctx, status, watch);
    }

    [SlashCommand("removecat", "Removes the specified cat from the database")]
    public async Task RemoveCat(InteractionContext ctx, 
    [ChoiceProvider(typeof(CatChoiceProvider))]
    [Option("Name", "The name of the cat")] string name){
        Status status = Status.SUCCESS;
        Stopwatch watch = new Stopwatch();
        watch.Start();

        string content = "";
        Cat cat = DatabaseReader.LoadCat(name);

        if(ctx.Channel.GuildId == null){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
            return;
        }

        try{
            if(cat.user != ctx.User.Id){
                Permissions permissions = ctx.Guild.GetMemberAsync(ctx.User.Id).Result.Permissions;
                if(!permissions.HasPermission(Permissions.Administrator)){
                    status = Status.FAILURE;
                    watch.Stop();
                    Logger.LogCommand("removecat", ctx, status, watch);

                    throw new InvalidOperationException("You cannot remove cats that are not yours!");
                }
            }

        }catch(InvalidOperationException e){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(e.Message));
            status = Status.ERROR;
            watch.Stop();
            Logger.LogCommand("removecat", ctx, status, watch);

            return;
        }

        if(DatabaseWriter.RemoveCat(name)){
            await Bot.bot.OnCatRemoved(cat,ctx);
            content = $"Succesfully removed {name}!";
            await Bot.client.GetSlashCommands().RefreshCommands();
            // CatChoiceProvider.RefreshDatabase();
        } else {
            content = $"Problem in trying to remove {name}.";
            status = Status.FAILURE;
        }
        

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(content));

        watch.Stop();
        Logger.LogCommand("removecat", ctx, status, watch);

        
    }

    [SlashCommand("set_available", "Tells others that your cat is available to RP with!")]
    public async Task SetAvailable(InteractionContext ctx,
    [ChoiceProvider(typeof(CatChoiceProvider))]
    [Option("name", "The name of your cat!")] string name){

        Status status = Status.SUCCESS;

        if(ctx.Channel.GuildId == null){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
            status = Status.FAILURE;
            Logger.LogCommand("set_available", ctx, status);
            return;
        }

        Cat cat = DatabaseReader.LoadCat(name);
        if(cat.user != ctx.User.Id){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You cannot do this for a cat that's not yours!"));
            status = Status.FAILURE;
            Logger.LogCommand("set_available", ctx, status);
            return;
        }

        cat.avaible_for_rp = !cat.avaible_for_rp;
        DatabaseWriter.SaveCat(cat);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Succesfully changed {cat.GetName()}'s looking for RP status to {cat.avaible_for_rp}!"));

        Logger.LogCommand("set_available", ctx, status);
    }

    [SlashCommandPermissions(Permissions.Administrator)]
    [SlashCommand("ageup", "Ages up all of the cat's within the database.")]
    public async Task AgeUp(InteractionContext ctx,
    [Option("Time", "The amount you want to age up. Default is 1 Moon")] long time = 1){
        if(ctx.Channel.GuildId == null){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
            Logger.LogCommand("ageup", ctx, Status.FAILURE);

            return;
        }

        List<Cat> cats = DatabaseReader.LoadCats();

        List<Cat> kits_to_paws = new List<Cat>();
        List<Cat> paws_to_wars = new List<Cat>();
        List<Cat> wars_to_elds = new List<Cat>();
        List<Cat> errors = new List<Cat>();

        foreach(Cat cat in cats){
            int age = cat.AgeUp((int)time);

            Rank? cat_rank = cat.GetRank();
            if(cat_rank == Rank.Kit && age >= 6){
                kits_to_paws.Add(cat);
            } else
            if((cat_rank == Rank.Apprentice || cat_rank == Rank.Medic_App || cat_rank == Rank.Messenger_App) && age >= 12 && age <=16){
                paws_to_wars.Add(cat);
            } else
            if(age == -1 || cat_rank == null){
                errors.Add(cat);
            }

            DatabaseWriter.SaveCat(cat);
        }

        string kits_to_paws_desc = "";
        if(kits_to_paws.Count == 0){
            kits_to_paws_desc = "None";
        }
        foreach(Cat cat in kits_to_paws){
            kits_to_paws_desc += cat.GetCondensedString();
        }

        string paws_to_wars_desc = "";
        if(paws_to_wars.Count == 0){
            paws_to_wars_desc = "None";
        }
        foreach(Cat cat in paws_to_wars){
            paws_to_wars_desc += cat.GetCondensedString();
        }

        string errors_desc;
        if(errors.Count == 0){
            errors_desc = "None";
        } else {
            errors_desc = "I could not update these cats! (Maybe their age is missing?):\n";
        }

        foreach(Cat cat in errors){
            errors_desc += cat.GetCondensedString();
        }

        

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
            Title = "__Age Up Results__",
            Description = $"Successfully updated the age of {cats.Count - errors.Count} cats!"
        };

        embed.AddField("Kits to Promote", kits_to_paws_desc);
        embed.AddField("Paws to Promote", paws_to_wars_desc);

        embed.AddField("Failures", errors_desc);

        embed.WithTimestamp(DateTimeOffset.Now);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        Logger.LogCommand("ageup", ctx, Status.SUCCESS);
    }

    [SlashCommandPermissions(DSharpPlus.Permissions.Administrator)]
    [SlashCommand("rankup", "Ranks up the specific cat.")]
    public async Task RankUp(InteractionContext ctx,
    [ChoiceProvider(typeof(CatChoiceProvider))]
    [Option("Name", "The name of the cat to rank up")] string name,
    [Option("Newname", "The new warrior name of the cat. Leave blank for kits->paws")] string new_name = ""){
        if(ctx.Channel.GuildId == null){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
            Logger.LogCommand("rankup", ctx, Status.FAILURE);

            return;
        }

        Cat cat = DatabaseReader.LoadCat(name);
        DatabaseWriter.RemoveCat(name);

        string old_cat = cat.GetCondensedString();

        void UpdateCat(Rank rank){
            if(new_name != ""){
                cat.EditName(new_name);
            } else {
                if(rank == Rank.Kit){
                    cat.EditName(cat.GetName().Replace("kit", "paw"));
                }
            }
            cat.EditRank(rank);
        }

        Rank? current_rank = cat.GetRank();
        if(current_rank == Rank.Kit){
            UpdateCat(Rank.Apprentice);
        } else
        if(current_rank == Rank.Apprentice){
            UpdateCat(Rank.Warrior);
        } else 
        if(current_rank == Rank.Medic_App){
            UpdateCat(Rank.Medic);
        } else
        if(current_rank == Rank.Messenger_App){
            UpdateCat(Rank.Messenger);
        } else {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Problem ranking up cat. Could not find rank to rank up to!"));
            return;
        }

        DatabaseWriter.SaveCat(cat);

        DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
            Title = "__Rank Up Results__"
            ,
            Description = old_cat + "\n" + cat.GetCondensedString()
        };

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        Logger.LogCommand("rankup", ctx, Status.SUCCESS);


    }

}