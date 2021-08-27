using System;
using System.Collections.Generic;

namespace DiscordEventSignupBot
{
    class GamerEvent
    {
        public string Name;
        public DateTime Time;
        public ulong MessageID;
        public Dictionary<ulong, AmIComing> Foo = new Dictionary<ulong, AmIComing>();

        public override string ToString()
        {
            return $"{Name} {Time}";
        }
    }

    enum AmIComing
    {
        Coming,
        Tentative,
        NotComing,
    }
}
