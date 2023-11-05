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

namespace Commands{

    public class SubmissionCommands : ApplicationCommandModule {

        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashCommand("submission_message", "Sets up the submission message if it has been deleted")]
        public async Task CharacterInfo(InteractionContext ctx,
        [Option("channel", "Discord Channel to send the message in.")] DiscordChannel channel){
            if(ctx.Channel.GuildId == null){
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("This Command is not enabled in DMs"));
                return;
            }

            DiscordEmbedBuilder embed_builder = new DiscordEmbedBuilder(){
                Title = "Submissions! :3",
                Description = "Click this button to open a channel where you can submit your cat and have them reviewed!"
            };

            DiscordMessageBuilder message_builder = new DiscordMessageBuilder().AddEmbed(embed_builder.Build())
            .AddComponents(new DiscordButtonComponent(ButtonStyle.Success, "create_submissions_channel", "Submit!"));

            await channel.SendMessageAsync(message_builder);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Sent submissions message!"));
        }

    }

}