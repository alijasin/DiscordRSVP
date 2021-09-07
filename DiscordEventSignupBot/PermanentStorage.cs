using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscordEventSignupBot
{
    class PermanentStorage
    {
        private static string StoragePath = @"D:/dev/.teststorage";
        private static object FileLock = new object();

        public static StorageRoot Read()
        {
            lock (FileLock)
            {
                if (!File.Exists(StoragePath))
                {
                    return new StorageRoot();
                }

                var text = File.ReadAllText(StoragePath);
                return JsonConvert.DeserializeObject<StorageRoot>(text);
            }
        }

        public static bool Write(Action<StorageRoot> writeAction)
        {
            lock (FileLock)
            {
                StorageRoot root;
                if (!File.Exists(StoragePath))
                {
                    root = new StorageRoot();
                }
                else
                {
                    var text = File.ReadAllText(StoragePath);
                    root = JsonConvert.DeserializeObject<StorageRoot>(text);
                }

                try
                {
                    writeAction(root);
                }
                catch (Exception e)
                {
                    Log.Write(e.ToString());
                    return false;
                }

                var writeText = JsonConvert.SerializeObject(root);
                File.WriteAllText(StoragePath, writeText);

                return true;
            }
        }
    }

    class StorageRoot
    {
        public List<GamerEvent> GamerEvents = new List<GamerEvent>();
        public DiscordInfo DiscordInfo = new DiscordInfo();
    }

    class DiscordInfo
    {
        public ulong GuildID;
        public ulong MentionRoleID;

        public string Token;

        public ulong ChannelID => ProdChannelID;
        public ulong DevChannelID;
        public ulong ProdChannelID;
    }
}
