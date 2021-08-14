using System.IO;
using Newtonsoft.Json;

namespace DiscordEventSignupBot
{
    class Config
    {
        private const string ConfigFilePath = "dicordinfo.cfg";

        public static Configuration Read { get; private set; }

        static Config()
        {
            Load();
        }

        public static void Load()
        {
            if (Read != null) { return; }

            if (File.Exists(ConfigFilePath))
            {
                Read = JsonConvert.DeserializeObject<Configuration>(
                    File.ReadAllText(ConfigFilePath));
            }
            else
            {
                Read = new Configuration();
            }
        }

        public static void Save()
        {
            File.WriteAllText(ConfigFilePath, JsonConvert.SerializeObject(Read));
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
