using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

//Since groups are stupid, we will just hold everything within here
public class HelpCommands{

    //These are relics, I decided to change how to do things.
    //This woulda made it easier to edit but I value readability over that
    ///show cat
    public const string SHOW_CAT_HELP_LONG = "This command takes in the name of the cat you want to display. About to roleplay with Featherpaw? Want to just check her bio out? You'd use \"/show cat Featherpaw\"!";
    public const string SHOW_CAT_HELP_SHORT = "**/show cat <name>**: Displays information about a singular cat.";

    //show cats
    public const string SHOW_CATS_HELP_LONG = "This command is the parent function for all /show commands but /show cat. Confused about what that means? Don't worry, I'll explain it to you!";
    public const string SHOW_CATS_HELP_SHORT = "**/show cats <rank> <clan> <user>**: Shows the specified cats with the given filters, or all cats if no filters are given.";

    
    public class HelpBase : ApplicationCommandModule {

        [SlashCommand("help", "Displays base help function to get you started with Lionpaw!")]
        public async Task DisplayHelp(InteractionContext ctx){

            bool staff = false;
            if(ctx.Channel.GuildId != null){
                if(ctx.Channel.Guild.GetMemberAsync(ctx.User.Id).Result.Permissions.HasPermission(Permissions.Administrator)){
                    staff = true;
                }
            }

            //We just want to build our help embed
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Lionpaw Help**",
                Description = "Mrrow! I know I can be a bit confusing at first, but I'll list an overview of all of the commands here! Want more information about a specific command? Unsure on how to use it? Just use /helpc <command> to learn more!"
            };

            List<Lionpaw.Channel> channels = Lionpaw.ChannelDatabase.LoadChannels();
            Lionpaw.Channel? submissions_channel = null;
            foreach(Lionpaw.Channel channel in channels){
                if(channel.channel_type == Lionpaw.ChannelType.SUBMISSION){
                    submissions_channel = channel;
                    break;
                }
            } 
            string submissions_channel_mention;

            if(submissions_channel == null){
                submissions_channel_mention = "<insert submissions channel link>";
            } else {
                IReadOnlyDictionary<ulong, DiscordChannel> guild_channels = ctx.Guild.Channels;
                submissions_channel_mention = guild_channels[submissions_channel.channel_id].Mention;
            }

            string show_string_description = $"Here we have all of the commands to display cats registered within the server! Want to register your cat? Check out {submissions_channel_mention}!\n";
            show_string_description += "\n**/show cat <name>**: Displays information about a singular cat.";
            show_string_description += "\n**/show cats <rank?> <clan?> <user?> <page?>**: Shows the specified cats with the given filters, or all cats if no filters are given.";
            show_string_description += "\n**/show all <page?>**: Shows all of the cats within the database.";
            show_string_description += "\n**/show user <user> <page?>**: Shows all of the cats belonging to the specific user.";
            show_string_description += "\n**/show rank <rank> <page?>**: Shows all of the cats that are at the current rank.";
            show_string_description += "\n**/show clan <clan> <page?>**: Shows all of the cats belonging to the specific clan";
            embed.AddField("__Show Commands__", show_string_description);

            //If they're a staff let's have Lionpaw show all of the staff commands
            if(staff){
                string cat_string_description = "Mrreow! I'll list all of the commands that edit our database for you too!\n";
                cat_string_description += "\n**/addcat <user> <link> <name?> <age?> <gender?> <rank?> <clan?> <auto find member?>**: Adds the cat to the database";
                cat_string_description += "\n**/editcat <name> <link?> <new name?> <age?> <gender?> <rank?> <clan?>**: Edits the cat with the specific parameters";
                cat_string_description += "\n**/removecat <name>**: Removes the cat from the database";
                cat_string_description += "\n**/ageup <amount?>**: Ages all cats up by the amount";
                cat_string_description += "\n**/rankup <name> <new name?>**: Ranks up the specific cat";
                embed.AddField("__Database Commands__", cat_string_description);
            }

            // embed.WithThumbnail(ctx.Client.CurrentUser.AvatarUrl);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed.Build()));
        }

        
        
    }

    [SlashCommandGroup("helpc", "Gives more descriptive information about each command")]
    public class HelpDescriptive : ApplicationCommandModule{
        public enum ShowEnum{
            [ChoiceName("cat")]
            cat,
            [ChoiceName("cats")]
            cats,
            [ChoiceName("user")]
            user,
            [ChoiceName("rank")]
            rank,
            [ChoiceName("clan")]
            clan,
            [ChoiceName("all")]
            all
        }

        [SlashCommand("show", "Displays information about each show command.")]
        public async Task DisplayShowHelp(InteractionContext ctx,
        [Option("Command", "The command to display more information about")] ShowEnum command){

            //We'll just write our own functions to make it cleaner
            if(command == ShowEnum.cat){
                await HelpCat(ctx);
            } else
            if(command == ShowEnum.cats){
                await HelpCats(ctx);
            } else 
            if(command == ShowEnum.user){
                await HelpUser(ctx);
            } else 
            if(command == ShowEnum.rank){
                await HelpRank(ctx);
            } else 
            if(command == ShowEnum.clan){
                await HelpClan(ctx);
            } else {
                await HelpAll(ctx);
            }

        }

        private async Task RespondWithEmbed(InteractionContext ctx, DiscordEmbed embed){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
        }

        private async Task HelpCat(InteractionContext ctx){
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Show Cat Help**",
                Description = "This is where we can display specific information about someone's kitty! Let me walk you through how to use it quickly, since this is one of the most used commands."
            };

            string usage_description = "**/show cat <name>**: Here we have a paramater <name>, which is simply the name of the cat you want to look up! ";
            usage_description += "For example, if you wanted to look up Featherpaw, you can do \"/show cat Featherpaw\", which will bring up a little embed giving you her bio as well as a link to her original doc. ";
            usage_description += "While the command is input sensitive (it doesn't respond well to typos!) it is not case sensitive, so \"/show cat FeAtHeRpAw\" will work too!";
            embed.AddField("__Usage__", usage_description);

            await RespondWithEmbed(ctx, embed);
        }

        private async Task HelpCats(InteractionContext ctx){
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Show Cat Help**",
                Description = "This command is the parent function for all /show commands but /show cat. Confused about what that means? Don't worry, I'll explain it to you!"
            };

            string usage_description = "**/show cats <rank?> <clan?> <user?> <page?>**: As we can see, all of the parameters within /show cats are optional (as indicated by the \"?\"). This is because using /show cats ";
            usage_description += "without any parameters is the same as using \"/show all\"! Similarly, only inputting a rank is the same as using \"/show rank\" and same with clan and user.\n\n";
            usage_description += "So, what is the purpose of /show cats? Well, if you want to filter more specificly! Say you want to see all of my cats within Cats Among the Thickening Mist, ";
            usage_description += "we'd do \"/show cats @Lionpaw Cats Among the Thickening Mist\". This will give you all of my cats within that specific clan!\n\n";
            usage_description += "There can be a lot of cats, even when filtered down. If you weant to scroll through, just use the page parameter to do so! The embed should tell you how many ";
            usage_description += "pages there are at the bottom. Going over will just cycle through.";
            embed.AddField("__Usage__", usage_description);

            await RespondWithEmbed(ctx, embed);
        }

        private async Task HelpAll(InteractionContext ctx){
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Show All Help**",
                Description = "This command will list every cat registered within the database. It lists 20 cats per page - just change the page if you want to scroll through them!"
            };

            string usage_description = "**/show all <page?>**: This command is fairly simple in its function. All you need to do is type \"/show all\" and it will display the first page of ";
            usage_description += "all of the cats registered within Starswept Tides! They are unfiltered as well as unorganized (they're ordered by when they were entered into the database), but it's there to ";
            usage_description += "look through!";
            embed.AddField("__Usage__", usage_description);

            await RespondWithEmbed(ctx, embed);
        }

        private async Task HelpUser(InteractionContext ctx){
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Show User Help**",
                Description = "Shows all of the cats belonging to a specific user! It will also provide a link to the doc of each cat."
            };

            string usage_description = "**/show user <user> <page?>**: Shows the cats belonging to the user specified within the user argument. Want to see what cats someone has? ";
            usage_description += "If it was Noca, you would just use \"/show user @NocaToca\"! It will showcase all of Noca's cats with a link to them as well.";
            embed.AddField("__Usage__", usage_description);

            await RespondWithEmbed(ctx, embed);
        }

        private async Task HelpRank(InteractionContext ctx){
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Show Rank Help**",
                Description = "Shows all of the cats that have that specific rank. It doesn't filter for clans, but can be very useful to see who the HR cats are - like Leaders!"
            };

            string usage_description = "**/show rank <rank> <page?>**: Using the rank specified within the rank argument, it showcases all of the cats that have that rank. For example, if you wanted to ";
            usage_description += "see who all of the medicine cats are, you can just do \"/show rank Medicine Cat\". This will give all of the medicine cats registered within the server!";
            embed.AddField("__Usage__", usage_description);

            await RespondWithEmbed(ctx, embed);
        }

        private async Task HelpClan(InteractionContext ctx){
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(){
                Title = "**Show Clan Help**",
                Description = "Shows all of the cats that are in the specified clan."
            };

            string usage_description = "**/show clan <clan> <page?>**: Using the clan specified within the clan argument, it showcases all of the cats that have that clan. For example, if you wanted to ";
            usage_description += "see all of the cats in Cats Among the Thickening Mist, you can just do \"/show clan Cats Among the Thickening Mist\". This will give all of the cats registered in Cats Among the Thickening Mist within the server!";
            embed.AddField("__Usage__", usage_description);

            await RespondWithEmbed(ctx, embed);
        }

    }
}


//Sets up our default strings for system messages
public class StringCommands : ApplicationCommandModule{

    [SlashCommandPermissions(Permissions.Administrator)]
    [SlashCommand("set_string", "Sets the keyed string to be the specific type.")]
    public async Task SetString(InteractionContext ctx,
    [Option("key", "The key name of the string to set")] string key,
    [Option("value", "The value that you want assoiciated with that key")] string value){

        StringDatabase.SaveString(key, value);

        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Saved string!"));

    }

    [SlashCommandPermissions(Permissions.Administrator)]
    [SlashCommand("dump_log", "Dumps the log into a text file and resets it.")]
    public async Task Log(InteractionContext ctx){
        if(ctx.Channel.GuildId == null && ctx.User.Id != 215975377770774528){
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
            Logger.LogCommand("dump_log", ctx, Status.FAILURE);

            return;
        }

        DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder();
        builder.WithContent("Meow! Here's the log file.");

        using(FileStream fs = new FileStream("Logger/log.txt", FileMode.Open)){
            builder.AddFile("Logger/log.txt", fs);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
        }

        Logger.Flush();

    }

    
}