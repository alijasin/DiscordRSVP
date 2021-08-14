using System.Threading;

namespace DiscordEventSignupBot
{
    class Program
    {
        static void Main(string[] args)
        {
            DiscordInterface.Start();

            // Keep thread alive.
            Thread.Sleep(Timeout.Infinite);
        }
    }
}