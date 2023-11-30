

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Lionpaw{

    namespace Messages{

        //Messages will be bigger once we add custom variables and such
        public class Message{

            private string message;

            public Message(string content){
                message = content;
            }

            public string GetContent(){
                return message;
            }
        }

        public enum MessageType{
            [ChoiceName("Welcome")]
            WELCOME,
            [ChoiceName("Goodbye")]
            GOODBYE,
            [ChoiceName("Submissions")]
            SUBMISSION,
            [ChoiceName("Submissions Channel")]
            SUBMISSION_CHANNEL
        }

        public static class MessageTypeExtender{

            public static DiscordEmbedBuilder CreateEmbed(this MessageType type, string guild_name){
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder();
                switch(type){
                    case MessageType.WELCOME:
                        embed.WithTitle("**✧⋄⋆⋅⋆⋄Welcome to" + guild_name+"⋄⋆⋅⋆⋄✧**");
                        return embed;
                    case MessageType.GOODBYE:
                        embed.WithTitle("User has left " + guild_name);
                        return embed;
                    default:
                        return embed;

                }
            }

        }

    }

}