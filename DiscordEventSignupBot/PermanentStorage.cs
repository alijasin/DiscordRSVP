using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DiscordEventSignupBot
{
    class PermanentStorage
    {
        private static string StoragePath = @"./.teststorage";
        private static string StoragePathx = @"./x.teststorage";
        private static object FileLock = new object();

        private static string EncryptionPassword;

        public static void SetPassword(string password)
        {
            EncryptionPassword = password;
        }

        public static StorageRoot Read()
        {
            lock (FileLock)
            {
                return ReadInternal();
            }
        }

        private static StorageRoot ReadInternal()
        {
            var textDirty = PSAes.Decrypt(File.ReadAllBytes(StoragePathx), EncryptionPassword);
            var textClean = textDirty.Substring(textDirty.IndexOf("{"));
            return JsonConvert.DeserializeObject<StorageRoot>(textClean);
        }

        public static bool Write(Action<StorageRoot> writeAction)
        {
            lock (FileLock)
            {
                var root = ReadInternal();

                try
                {
                    writeAction(root);
                }
                catch (Exception e)
                {
                    Log.Write(e.ToString());
                    return false;
                }

                // Aes encryption mangles the first five or so characters and I don't know why.
                // As such; append a bunch of spaces to the start;
                var writeText = "                          " + JsonConvert.SerializeObject(root);
                var ec = PSAes.Encrypt(writeText, EncryptionPassword);
                File.WriteAllBytes(StoragePathx, ec);

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
