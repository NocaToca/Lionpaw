
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System.Diagnostics;

[SlashCommandGroup("show", "Shows relative information about cats")]
public class DisplayCommands : ApplicationCommandModule {

    //Filters by rank, clan and user
    //User will be the bot if it is not supplied! The bot doesn't have any cats, and will be the same in the ctx, so it's easily detectable
    public async Task DisplayCatsWithParams(InteractionContext ctx, Rank rank, Clan clan, DiscordUser user, int page, bool open_to_rp = false){

        Stopwatch watch = new Stopwatch();
        watch.Start();

        //Grab all cats then make our dummy list
        List<Cat> all_cats = DatabaseReader.LoadCats();
        List<Cat> filtered_cats = new List<Cat>();

        //If we have all default settings, we're not filtering
        if(clan == Clan.None && rank == Rank.None && ctx.Client.CurrentUser.Id == user.Id){
            filtered_cats = all_cats;
        } else {
            //Then we go and filter
            foreach(Cat cat in all_cats){

                //This is so we can control what we're filtering by
                Rank? temp_rank = rank;
                Clan? temp_clan = clan;
                ulong id = user.Id;
                bool temp_open = false;

                if(rank == Rank.None){
                    temp_rank = cat.GetRank();
                }
                if(clan == Clan.None){
                    temp_clan = cat.GetClan();
                }
                if(ctx.Client.CurrentUser.Id == id){
                    id = cat.user;
                }
                if(open_to_rp){
                    temp_open = true;
                }

                if(cat.GetRank() == temp_rank && cat.GetClan() == temp_clan && id == cat.user){
                    if(temp_open){
                        if(cat.avaible_for_rp){
                            filtered_cats.Add(cat);
                        }
                    } else {
                        filtered_cats.Add(cat);
                    }
                }
            }
        }

        //Reason I don't do an array is bc this can be twenty or less. We don't know!
        List<Cat> cats_to_display = new List<Cat>();


        //Now we just go through filtered cats by page
        int page_number;
        int amount_of_cats_to_display;
        page_number = (page > (int)Math.Ceiling(filtered_cats.Count / 20.0)) ? page % (int)Math.Ceiling(filtered_cats.Count / 20.0) : page;
        amount_of_cats_to_display = (page_number * 20 > filtered_cats.Count) ? filtered_cats.Count - ((page_number - 1) * 20) : 20;



        for (int i = (page - 1) * 20, k = 0; k < amount_of_cats_to_display && i < filtered_cats.Count; i++, k++){ 
            cats_to_display.Add(filtered_cats[i]);
        }

        //Build our description based off of the cats we've filtered
        string descritpion = "";
        foreach(Cat cat in cats_to_display){
            descritpion += cat.GetCondensedString();
        }

        //Base title, url, and color
        string title = "Cats of the Starswept Tides";
        string url;

        #pragma warning disable 0168
        try{
            url = ctx.Channel.Guild.IconUrl;
        }catch(Exception e){
            url = "";
        }
        #pragma warning restore

        DiscordColor color = DiscordColor.Gray;

        //If we're filtering by clan, we can set out params correctly
        if(clan != Clan.None){
            title = Cat.GetClanString(clan);
            color = DiscordUtils.GetColorFromClan(clan);
            url = DiscordUtils.GetClanImage(clan);
        }

        //Let's make it be like "Warriors of the Thickening Mist. We just have to get rid of the word "Cats"
        if(rank != Rank.None){
            title = title.Replace("Cats","");
            title = Cat.GetRankString(rank) + title;
        }

        //We'll handle the user later, so we can add buttons. Let's build our base embed first
        DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
            Title = title,
            Description = descritpion
        };
        

        DiscordInteractionResponseBuilder response = new DiscordInteractionResponseBuilder();

        //Now if we filtered by user let's add our links.
        if(user.Id != ctx.Client.CurrentUser.Id){
            url = user.AvatarUrl;
            embed.WithThumbnail(url);

            for(int i = 0; i < cats_to_display.Count; i++){
                //5 max
                //
                int size = (cats_to_display.Count - i < 5) ? cats_to_display.Count - i : 5;

                DiscordComponent[] components = new DiscordComponent[size];
                for(int k = 0; i < cats_to_display.Count && k < 5; k++, i++){
                    components[k] = new DiscordLinkButtonComponent(cats_to_display[i].GetLink(), $"{cats_to_display[i].GetName()}!");
                }

                response.AddComponents(components);
            }

            if(user.BannerColor != null){
                embed.WithColor((DiscordColor)user.BannerColor);
            }

        } else {
            if(url != ""){
                embed.WithThumbnail(url);
            }
            embed.WithColor(color);
        }
        embed.WithFooter($"Page {page}/{Math.Ceiling(filtered_cats.Count/20.0)}");

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response.AddEmbed(embed.Build()));
        watch.Stop();

        Logger.LogCommand("show", ctx, Status.SUCCESS, watch);
    }

        //Works for a single cat
        [SlashCommand("cat", "Displays information about the given cat.")]
        public async Task DisplayCat(InteractionContext ctx, 
        [ChoiceProvider(typeof(CatChoiceProvider))]
        [Option("Name", "The name of the cat")] string name){
            try{
                Cat cat = DatabaseReader.LoadCat(name);

                DiscordEmbed embed;
                if(ctx.Channel.GuildId == null){
                    embed = cat.BuildEmbed(null);
                } else {
                    embed = cat.BuildEmbed(ctx.Channel.Guild);
                }

                
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed).AddComponents(
                    new DiscordLinkButtonComponent(cat.GetLink(), "Link!")
                ));
            } catch(Exception e){
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Failure to display cat: " + e.Message));
            }
            
        }

        #pragma warning disable 8625
        //Permutation functions
        [SlashCommand("cats", "Displays all cats related to the given parameters.")]
        public async Task DisplayCats(InteractionContext ctx, [Option("Rank", "The rank of the cats.")] Rank rank = Rank.None, [Option("Clan", "The clan of the cats")] Clan clan = Clan.None,
        [Option("User", "The user the cats belong to.")] DiscordUser user = null,
        [Option("Page", "The page number to display the cats")] long page = 1){
            if(user == null){
                user = ctx.Client.CurrentUser;
            }

            await DisplayCatsWithParams(ctx, rank, clan, user, (int)page);
        }
        #pragma warning restore
    
        [SlashCommand("open", "Displays your cats that are open to RP!")]
        public async Task DisplayOpen(InteractionContext ctx){
            
            await DisplayCatsWithParams(ctx, Rank.None, Clan.None, ctx.User, 1, true);
        }

    

        
        [SlashCommand("all", "Displays all currently stored cats")]
        public async Task DisplayAllCats(InteractionContext ctx,
        [Option("Page", "The page number to display the cats")] long page = 1){

            await DisplayCatsWithParams(ctx, Rank.None, Clan.None, ctx.Client.CurrentUser, (int)page);
        }

        [SlashCommand("rank", "Displays all cats with the given rank")]
        public async Task DisplayCatRank(InteractionContext ctx, [Option("Rank", "The rank of the cats.")] Rank rank,
        [Option("Page", "The page number to display the cats")] long page = 1){

            await DisplayCatsWithParams(ctx, rank, Clan.None, ctx.Client.CurrentUser, (int)page);
        }

        [SlashCommand("clan", "Displays all cats with the given clan.")]
        public async Task DisplayCatClan(InteractionContext ctx, [Option("Clan", "The clan of the cats")] Clan clan,
        [Option("Page", "The page number to display the cats")] long page = 1){

            await DisplayCatsWithParams(ctx, Rank.None, clan, ctx.Client.CurrentUser, (int)page);

        }


        [SlashCommand("user", "Displays all cats belonging to the provided user")]
        public async Task DisplayCatsUser(InteractionContext ctx, [Option("User", "The owner of the Characters!")] DiscordUser user,
        [Option("Page", "The page number to display the cats")] long page = 1){

            await DisplayCatsWithParams(ctx, Rank.None, Clan.None, user, (int)page);

        }
    

}