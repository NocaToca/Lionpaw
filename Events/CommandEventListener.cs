
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Newtonsoft.Json;

//This class sets up event listeners for each command, so we can tack on additional commands
public partial class Bot{

    public async Task OnCatAdded(Cat cat_added, InteractionContext ctx){
        try{
            IReadOnlyDictionary<ulong, DiscordRole> roles = ctx.Guild.Roles;
            foreach(KeyValuePair<ulong, DiscordRole> pair in roles){
                string role_name = pair.Value.Name;
                string clan_name = Cat.GetClanStringShort(cat_added.GetClan());
                string rank_name = Cat.GetRankString(cat_added.GetRank());

                if(Regex.Match(role_name, clan_name, RegexOptions.IgnoreCase).Success &&
                Regex.Match(role_name, rank_name, RegexOptions.IgnoreCase).Success){
                    await ctx.Channel.Guild.GetMemberAsync(ctx.User.Id).Result.GrantRoleAsync(pair.Value);
                    break;
                }
            }
        }catch(Exception e){
            Logger.Error("Exception in OnCatAdded event: " + e.Message);
        }
    }

    public async Task OnCatRemoved(Cat removed_cat, InteractionContext ctx){
        try{
            IReadOnlyDictionary<ulong, DiscordRole> roles = ctx.Guild.Roles;
            foreach(KeyValuePair<ulong, DiscordRole> pair in roles){
                string role_name = pair.Value.Name;
                string clan_name = Cat.GetClanStringShort(removed_cat.GetClan());
                string rank_name = Cat.GetRankString(removed_cat.GetRank());

                if(Regex.Match(role_name, clan_name, RegexOptions.IgnoreCase).Success &&
                Regex.Match(role_name, rank_name, RegexOptions.IgnoreCase).Success){
                    await ctx.Channel.Guild.GetMemberAsync(ctx.User.Id).Result.RevokeRoleAsync(pair.Value);
                    break;
                }
            }
        }catch(Exception e){
            Logger.Error("Exception in OnCatAdded event: " + e.Message);
        }
    }

}
