using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordEventSignupBot
{
    static class Log
    {
        private static ConsoleLogger ConsoleLog;

        static Log()
        {
            ConsoleLog = new ConsoleLogger();
        }

        public static void Write(string s)
        {
            ConsoleLog.Log($"{DateTime.Now}: {s}");
        }
    }

    interface Logger
    {
        void Log(string message);
    }

    class ConsoleLogger : Logger
    {
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
