
using DSharpPlus;
using DSharpPlus.Entities;

namespace DiscordExtensions{

        
    public static class DiscordUtils{
        

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


    }

}