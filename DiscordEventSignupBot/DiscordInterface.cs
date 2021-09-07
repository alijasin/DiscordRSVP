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

        public static SocketUser GetUserInfo(ulong userID)
        {
            return Client.GetUser(userID);
        }

        public static IMessage GetMessage(ulong raidID)
        {
            var discordInfo = PermanentStorage.Read().DiscordInfo;
            return Client.GetGuild(discordInfo.GuildID)
                       .GetTextChannel(discordInfo.ChannelID)
                       .GetMessageAsync(raidID).Result;
        }

        private static async Task SetupBot()
        {
            Client = new DiscordSocketClient();

            var readyGate = new ManualResetEventSlim();
            Client.Ready += () =>
            {
                readyGate.Set();
                return null;
            };

            var discordInfo = PermanentStorage.Read().DiscordInfo;

            await Client.LoginAsync(TokenType.Bot, discordInfo.Token);
            await Client.StartAsync();

            readyGate.Wait();

            TextChannel = Client
                .GetGuild(discordInfo.GuildID)
                .GetTextChannel(discordInfo.ChannelID);

            Client.MessageReceived += async (message) =>
            {
                var messageString = message.ToString().Trim();

                //Don't listen to non-command messages nor bot's own messages.
                if (messageString.StartsWith('!') && !message.Author.IsBot)
                {
                    Commands.Invoke(new CommandParamaters(messageString.Substring(1)));
                    await message.DeleteAsync();
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
                            }
                            else if (reactionInfo.Emote.Name == Reactions.Tentative.Name)
                            {
                                ge.Foo[reactionInfo.UserId] = AmIComing.Tentative;
                            }
                            else if (reactionInfo.Emote.Name == Reactions.Declined.Name)
                            {
                                ge.Foo[reactionInfo.UserId] = AmIComing.NotComing;
                            }
                            else
                            {
                                Log.Write(reactionInfo.Emote.Name);
                            }

                        }
                    }

                });

#if false
                await msg.ModifyAsync(msgProps =>
                {
                    msgProps.Content = "XD";
                });
#endif
            };

            Log.Write("Connected to Discord.");
        }
    }
}
