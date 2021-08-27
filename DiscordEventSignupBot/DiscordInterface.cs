using System;
using System.Collections.Generic;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using Discord.Rest;
using System.Threading;

namespace DiscordEventSignupBot
{
    static class DiscordInterface
    {
        //Init the emojis and emotes 
        private static Dictionary<string, IEmote> emotes = new Dictionary<string, IEmote>()
            {
                {"SignedUp", Emote.Parse("<:SignedUp:873738981022515261>")},
                {"Absent", Emote.Parse("<:Absent:873737907704320051>")},
                {"Late", Emote.Parse("<:Late:873739705164914758>")},
            };

        public static DiscordSocketClient Client { get; private set; }
        private static SocketTextChannel TextChannel;

        public static void Start()
        {
            var setupGate = new ManualResetEventSlim();
            new Thread(() =>
            {
                SetupBot().GetAwaiter().GetResult();
                setupGate.Set();
            }).Start();
            setupGate.Wait();
        }

        public static RestUserMessage SendMessage(string text)
        {
            try
            {
                return TextChannel.SendMessageAsync(text).Result;
            }
            catch (Exception e)
            {
                Log.Write("Failed to send message.");
                Log.Write(e.Message);
                return null;
            }
        }

        private static async Task SetupBot()
        {
            Client = new DiscordSocketClient();

            await Client.LoginAsync(TokenType.Bot, Config.Read.Token);
            await Client.StartAsync();

            var readyGate = new ManualResetEventSlim();
            Client.Ready += () =>
            {
                readyGate.Set();
                return null;
            };

            readyGate.Wait();

            TextChannel = Client
                .GetGuild(Config.Read.GuildID)
                .GetTextChannel(Config.Read.ChannelID);

            Client.MessageReceived += async (message) =>
            {
                var messageString = message.ToString().Trim();

                //Don't listen to non-command messages nor bot's own messages.
                if (messageString.StartsWith('!') && !message.Author.IsBot)
                {
                    Commands.Invoke(new CommandParamaters(messageString.Substring(1)));
                }
            };

            Client.ReactionAdded += async (arg1, arg2, reactionInfo) =>
            {
                if (reactionInfo.User.Value.IsBot) { return; }

                PermanentStorage.Write(root =>
                {
                    foreach (var ge in root.GamerEvents)
                    {
                        if (reactionInfo.MessageId == ge.MessageID)
                        {
                            if (reactionInfo.Emote.Name == Reactions.SignedUp.Name)
                            {
                                ge.Foo[reactionInfo.UserId] = AmIComing.Coming;
                                SendMessage("Coomer detected");
                            }
                        }
                    }
                });
            };

            Log.Write("Connected to Discord.");
            SendMessage("Bot online. Type !help for a list of commands.");
        }

        private static Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            throw new NotImplementedException();
        }
    }
}
