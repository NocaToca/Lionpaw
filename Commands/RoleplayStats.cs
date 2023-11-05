

//Opt in roleplay stats commands
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Roleplay;

public class RegisterCommands : ApplicationCommandModule{

    [SlashCommand("create", "Creates an account to record stats!")]
    public async Task CreateAccount(InteractionContext ctx){

        if(RoleplayDatabase.NewUser(ctx.User.Id)){
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Added User!**"
            };

            string description = "Mrrow! Added you to my internal database to read through you replies!\n\n";
            description += "NOTE: In order to get stats for you I do have to collect your messages within the roleplay channels! I understand that this makes some people uncomfy, so you can just do /unregister ";
            description += "to undo this! This wipes your data within my internal storage and is irrevisable, so you will lose your stats if you had them. I don't collect or use your data for anything else, and you can ";
            description += "check yourself; most of my code can be looked through at: https://github.com/NocaToca/Lionpaw";

            embed.WithDescription(description);

            await ctx.CreateResponseAsync(embed, true);
        } else {
            await ctx.CreateResponseAsync("You already are registered!", true);
        }
    }

    [SlashCommand("unregister", "Deletes your account from the database")]
    public async Task Unregister(InteractionContext ctx){

        if(RoleplayDatabase.Unregister(ctx.User.Id)){
            await ctx.CreateResponseAsync("Unregistered!", true);
        } else {
            await ctx.CreateResponseAsync("I couldn't find you within my database. You probably aren't registered!", true);
        }
    }

    [SlashCommand("stats", "Shows your roleplay stats!")]
    public async Task ShowStats(InteractionContext ctx){
        if(RoleplayDatabase.IsUser(ctx.User.Id)){
            RoleplayStatistics? statistics = RoleplayDatabase.LoadStats(ctx.User.Id);
            if(statistics == null){
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("I couldn't seem to grab your information. Have you sent a roleplay message since registering?"));
                return;
            }
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = $"**{ctx.User.Username}\'s Starswept Tides stats!**",
                Description = "Mrrow! Here are some stats I collected from reading your amazing replies!"
            };
            /*
            if(free_data != null){
                //Free Data
                this.number_of_replies = free_data.number_of_replies;
                this.average_reply_length = free_data.average_reply_length;
                this.longest_message_length = free_data.longest_message;
            }
            */

            string basic_description;
            basic_description = $"Total number of replies: {statistics.Value.number_of_replies}\n";
            basic_description += $"Average reply length: {statistics.Value.average_reply_length:F2}\n";
            basic_description += $"Longest reply length: {statistics.Value.longest_message_length}\n";
            basic_description += $"Favorite character: (To be implemented)!";

            embed.AddField("__Replies__", basic_description);

            /*
                if(word_data != null){
                //Word Storage
                foreach(WordStorage.WordCounter counter in word_data.word_counters){
                    this.word_counter.Add(counter.word, counter.amount);
                }
                this.average_unique_words = word_data.average_unique_words;
                this.number_of_words = word_data.number_of_words;
                this.palindromes = word_data.palindromes;
                this.favorite_words = word_data.favorite_words;
            }   
            */
            string word_description;
            word_description = $"Average unique words per reply: {statistics.Value.average_unique_words:F2}!\n";
            word_description += $"Total number of words: {statistics.Value.number_of_words}!\n";
            word_description += $"\nFavorite words: {statistics.Value.favorite_words[0]}, {statistics.Value.favorite_words[1]}, {statistics.Value.favorite_words[2]}";

            embed.AddField("__Word Stats!__", word_description);

            /*
                if(alliteration_data != null){
                //Alliteration Data
                foreach(AlliterationData.AlliterationCounter counter in alliteration_data.alliteration_counter){
                    alliteration_counter.Add(counter.character, counter.amount);
                }
                this.alliteration_score = alliteration_data.alliteration_score;
            }
            */
            string misc_description;
            misc_description = $"Total alliteration score: {statistics.Value.alliteration_score:F2}\n";
            misc_description += "Puncuations Used: (To be implemented)";

            embed.AddField("__Misc Stats__", misc_description);
            embed.WithThumbnail(ctx.User.AvatarUrl);

            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));


        } else {
            await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You don't seem to be registered! Use /create to start tracking stats!"));
        }
    }

}