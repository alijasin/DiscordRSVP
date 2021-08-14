using System.Threading;

namespace DiscordEventSignupBot
{
    class Program
    {


        static void Main(string[] args)
        {
            Config.Load();

            var db = new DiscordBot();
            db.asd();

            // Keep thread alive.
            Thread.Sleep(Timeout.Infinite);
        }
    }
}