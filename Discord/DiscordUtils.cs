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
using System.Security.Cryptography.X509Certificates;

public static class DiscordUtils{

    public static string praire_image_url = "https://media.discordapp.net/attachments/1168749227158016140/1169434279839801455/72dd102b6c4c6a1bf4b02dcf57d5ee8f.jpg?ex=6555638b&is=6542ee8b&hm=7eeeac2e80ca759981f920504f03a35838dc53f157cbe50933d329d3f39769db&=&width=451&height=676";
    public static string woodlands_image_url = "https://media.discordapp.net/attachments/1168749227158016140/1169432980876439582/bced12336f13791a1120d015ff3f9f33.jpg?ex=65556255&is=6542ed55&hm=4221b596716f7d09e691d3cc0d2d1280dc771c5ec9aa5d654f0386ebd832d2e1&=&width=450&height=676";
    public static string mist_image_url = "https://media.discordapp.net/attachments/1168749227158016140/1169434632882753536/6b3dd2fd997b334b1f558f52ab78e5fd.jpg?ex=655563df&is=6542eedf&hm=b999e3b26b2ef1268dfc8c018d28ca690e121e4aeb257a81325901555f7f9702&=&width=541&height=676";

    public static string GetClanImage(Clan clan){
        switch(clan){
            case Clan.Mist:
                return mist_image_url;
            case Clan.Praire :
                return praire_image_url;
            case Clan.Woodlands:
                return woodlands_image_url;
            default:
                return "";
        }
    }
    

    public static async Task<DiscordChannel> CreatePrivateChannel(DiscordClient sender, DiscordMember user, DiscordGuild guild){
        string channel_name = $"{user.Username}_submission_channel";

        //Let's make sure that the channel doesnt already exist
        DiscordChannel? channel = null;

        foreach(KeyValuePair<ulong, DiscordChannel> other_channel in guild.Channels){
            if(other_channel.Value.Name == channel_name){
                channel = other_channel.Value;
                return channel;
            }
        }
        #pragma warning disable 
        if(channel == null){
            channel =  await guild.CreateTextChannelAsync(channel_name);
        }
        #pragma warning restore

        DiscordRole role = guild.GetRole(guild.Id);

        DiscordOverwriteBuilder[] builder = new DiscordOverwriteBuilder[]{ new DiscordOverwriteBuilder(role).Deny(Permissions.AccessChannels)  };

        await channel.ModifyAsync(x => x.PermissionOverwrites = builder);
        await channel.AddOverwriteAsync(user, Permissions.AccessChannels);

        await SendChannelDeleteMessage(channel, user, guild);
        return channel;
    }

    public static async Task SendChannelDeleteMessage(DiscordChannel channel, DiscordMember user, DiscordGuild guild){

        DiscordEmbedBuilder embed_builder = new DiscordEmbedBuilder(){
                Title = "Hello! :3",
                Description = "Press here to delete the channel and close submission!!"
            };

            DiscordMessageBuilder message_builder = new DiscordMessageBuilder().WithContent("Here is your submissions channel ").AddMention(new UserMention(user)).AddEmbed(embed_builder.Build())
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Danger, "delete_submissions_channel", "Close!"));

            await message_builder.SendAsync(channel);
    }

    public static async Task DeleteChannel(DiscordChannel channel){
        await channel.DeleteAsync();
    }

    public static DiscordColor GetColorFromClan(Clan? clan){
        if(clan == null){
            return DiscordColor.Gray;
        }

        if(clan == Clan.Woodlands){
            return new DiscordColor("#42663c");
        } else
        if(clan == Clan.Praire){
            return new DiscordColor("#d4f099");
        } else
        if(clan == Clan.Mist){
            return new DiscordColor("#9dd4e0");
        }

        return DiscordColor.Gray;
    }

}