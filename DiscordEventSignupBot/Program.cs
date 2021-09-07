using System.Collections.Generic;
using System.Threading;

namespace DiscordEventSignupBot
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordInterface.Start();
            //DiscordInterface.SendMessage("Please refrain from chirping in prod like a gaggle of fucken broads.");

            // Keep thread alive.
            Thread.Sleep(Timeout.Infinite);
        }
    }
}