using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lionpaw.Databases;
using Lionpaw.Statistics;

namespace Lionpaw{


    namespace Commands{

        public class StatisticsModule : Module{

            public StatisticsCommands statistics_commands;

            public StatisticsModule(Guild guild) : base(guild){
                statistics_commands = new StatisticsCommands(this);
            }

            [SlashCommandGroup("stats","Displays all information about statistics!")]
            public class StatisticsCommands : ApplicationCommandModule{

                public Module module;
                
                public StatisticsCommands(Module module){
                    this.module = module;
                }

                [SlashCommand("create", "Creates a statistics profile for the user")]
                public static async Task Create(InteractionContext ctx){
                    //We don't really need to communicate with the module
                    if(await StatisticsAccessor.IsUser(ctx.User.Id)){
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Already Registered!").AsEphemeral());
                    } else {
                        DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                            Title = "**Added User!**"
                        };

                        string description = "Mrrow! Added you to my internal database to read through you replies!\n\n";
                        description += "NOTE: In order to get stats for you I do have to collect your messages within the roleplay channels! I understand that this makes some people uncomfy, so you can just do /unregister ";
                        description += "to undo this! This wipes your data within my internal storage and is irrevisable, so you will lose your stats if you had them. I don't collect or use your data for anything else, and you can ";
                        description += "check yourself; most of my code can be looked through at: https://github.com/NocaToca/Lionpaw";

                        embed.WithDescription(description);

                        await ctx.CreateResponseAsync(embed, true);
                    }
                }

                [SlashCommand("delete", "Deletes your statistics profile.")]
                public static async Task Delete(InteractionContext ctx){
                    //We also do not have to interact with the module here
                    if(await StatisticsAccessor.IsUser(ctx.User.Id)){
                        await DatabaseAccessor.DeleteDirectory(DatabaseAccessor.STATISTICS_PATH(ctx.User.Id));
                        await ctx.CreateResponseAsync("Unregistered!", true);
                    } else {
                        await ctx.CreateResponseAsync("I couldn't find you within my database. You probably aren't registered!", true);
                    }
                }

                public enum ShowType{
                    [ChoiceName("All")]
                    ALL,
                    [ChoiceName("Server")]
                    SERVER
                }

                [SlashCommand("show", "Shows youre statistics profile.")]
                public static async Task Show(InteractionContext ctx,
                [Option("Scope", "The scope of the statistics you want to display")]ShowType type){
                    if(type == ShowType.ALL){
                        await Lionpaw.MainBot.CallCommandForGuild<StatisticsCommands>(ctx).ShowAll(ctx);
                    } else 
                    if(type == ShowType.SERVER){
                        await Lionpaw.MainBot.CallCommandForGuild<StatisticsCommands>(ctx).ShowServer(ctx);
                    }
                }

                private async Task ShowAll(InteractionContext ctx){
                    await ShowStatistics(await StatisticsAccessor.LoadAll(ctx.User.Id), ctx);
                }

                private async Task ShowServer(InteractionContext ctx){
                    await ShowStatistics(await StatisticsAccessor.LoadStatistics(ctx.User.Id, ctx.Guild.Id), ctx);
                }


                private async Task ShowStatistics(RoleplayStatistics? statistics, InteractionContext ctx){

                    if(await StatisticsAccessor.IsUser(ctx.User.Id)){
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
                        string name = "N/A";
                        try{
                            name = statistics.Value.favorite_character.OrderByDescending(kv => kv.Value).Take(1).ElementAt(0).Key;
                        }catch(Exception e){
                            await Logger.Error(e.Message);
                        }
                        basic_description += $"Favorite character: {name}!";

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
                        long times_used_period = 0, times_used_comman = 0, times_used_question = 0, times_used_exclamation = 0;
                        try{
                            times_used_period = statistics.Value.punctionation_counter['.'];
                        }catch{}
                        try{
                            times_used_comman = statistics.Value.punctionation_counter[','];
                        }catch{}
                        try{
                            times_used_question = statistics.Value.punctionation_counter['?'];
                        }catch{}
                        try{
                            times_used_exclamation = statistics.Value.punctionation_counter['!'];
                        }catch{}

                        string misc_description;
                        misc_description = $"Total alliteration score: {statistics.Value.alliteration_score:F2}\n";
                        misc_description += $"Puncuations Used: \nUsed '.' {times_used_period} times\nUsed ',' {times_used_comman} times\n";
                        misc_description += $"Used '?' {times_used_question} times\nUsed '!' {times_used_exclamation} times";

                        embed.AddField("__Misc Stats__", misc_description);
                        embed.WithThumbnail(ctx.User.AvatarUrl);

                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));

                    } else {
                        await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("You don't seem to be registered! Use /create to start tracking stats!"));
                    }

                    await Logger.LogCommand("stats", ctx);
                }

            }

        }
        
    }

}