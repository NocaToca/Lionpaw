

using DSharpPlus.Entities;

namespace Lionpaw{
    public partial class Lionpaw{

        public delegate Task CustomEvent(object arg, DiscordGuild guild, Lionpaw lionpaw);
        public CustomEvent OnCharacterAdded = async (o, g, l) =>{};
        public CustomEvent OnCharacterRemoved = async (o, g, l) =>{};

    }
}