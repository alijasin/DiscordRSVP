using System;
using System.Collections.Generic;
using System.Threading;

namespace DiscordEventSignupBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Please input password:");
            var password = Console.ReadLine();
            PermanentStorage.SetPassword(password);

            DiscordInterface.Start();
            //DiscordInterface.SendMessage("Please refrain from chirping in prod like a gaggle of fucken broads.");

            // Keep thread alive.
            Thread.Sleep(Timeout.Infinite);
        }
    }
}