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

        public static void Write(Action<StorageRoot> writeAction)
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

                writeAction(root);
                var writeText = JsonConvert.SerializeObject(root);

                File.WriteAllText(StoragePath, writeText);
            }
        }
    }

    class StorageRoot
    {
        public List<GamerEvent> GamerEvents = new List<GamerEvent>();
    }
}
