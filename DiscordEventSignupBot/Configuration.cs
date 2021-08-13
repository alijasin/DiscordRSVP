﻿using System.IO;
using Newtonsoft.Json;

namespace DiscordEventSignupBot
{
    class Config
    {
        private const string ConfigFilePath = "dicordinfo.cfg";

        public static Configuration DiscordInfo { get; private set; }

        public static void Load()
        {
            if (File.Exists(ConfigFilePath))
            {
                DiscordInfo = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigFilePath));
            }
            else
            {
                DiscordInfo = new Configuration();
            }
        }

        public static void Save()
        {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(DiscordInfo));
        }
    }

    class Configuration
    {
        public ulong GuildID; 
        public ulong ChannelID;
        public ulong MentionRoleID;

        public string Token;
    }
}
