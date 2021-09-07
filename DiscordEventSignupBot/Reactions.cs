using Discord;
using System.Collections.Generic;

namespace DiscordEventSignupBot
{
    static class Reactions
    {
        public static IEmote SignedUp = Emote.Parse("<:SignedUp:873738981022515261>");
        public static IEmote Declined = Emote.Parse("<:Absent:873737907704320051>");
        public static IEmote Tentative = Emote.Parse("<:Tentative:880938784458424320>");
    }
}
