using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DiscordEventSignupBot
{
    class Commands
    {
        
        // Command string are always stored and accessed in lower case.
        private static Dictionary<string, Action<CommandParamaters>> CommandLookup { get; } 
            = new Dictionary<string, Action<CommandParamaters>>();
        private static CommandContainer CommandContainerInstance { get; }
            = new CommandContainer();

        static Commands()
        {
            var commandMethods = typeof(CommandContainer).GetMethods();

            foreach (var method in commandMethods)
            {
                var callableCommandAttribute = method.CustomAttributes.FirstOrDefault(
                    atr => atr.AttributeType == typeof(CallableCommand));
                if (callableCommandAttribute != null)
                {
                    // This depends on no one adding anything to the constructor of
                    // CallableCommand. If something breaks relating to what strings
                    // map to what commands it's probably because the CallableCommand
                    // constructor takes something other than (string).
                    var commandName = (string)(callableCommandAttribute.ConstructorArguments[0].Value);

                    if (CommandLookup.ContainsKey(commandName))
                    {
                        throw new Exception("Two commands may not share the same name.");
                    }
                    CommandLookup[commandName.ToLower()] =  new Action<CommandParamaters>(
                        commandParams => method.Invoke(CommandContainerInstance, new[] { commandParams }));
                }
            }
        }

        public static void Invoke(CommandParamaters commandParamaters)
        {
            var commandName = commandParamaters.CommandWords[0].ToLower();
            if (CommandLookup.ContainsKey(commandName))
            {
                try
                {
                    CommandLookup[commandName](commandParamaters);
                }
                catch (Exception e)
                {
                    DiscordInterface.SendMessage(e.InnerException.Message);
                }
            }
        }
    }

    class CommandContainer
    {
        /**
         * 
         * 
         *      
         *      [CallableCommand("example")]
         *      public void IAmAnExample(CommandParamaters commandParamaters)
         *      {
         *          Console.WriteLine("In example.");
         *          Console.WriteLine($"commandParamaters.CompleteCommand: {commandParamaters.CompleteCommand}");
         *      
         *          // !example hello world
         *          // Output:
         *          // >In Example.
         *          // >commandParamaters.CompleteCommand: !example hello world
         *      }
        **/

        [CallableCommand("help")]
        public void CommandHelp(CommandParamaters commandParamaters)
        {
            DiscordInterface.SendMessage("Available commands: \n" +
                            "!create <Event name> <mon, tue, wed, thur, fri, sat, sun> <HH:MM> \n " +
                            "!roster <RaidID> \n");
        }

        [CallableCommand("create")]
        public void CommandCreate(CommandParamaters commandParamaters)
        {
            Dictionary<string, IEmote> emotes = new Dictionary<string, IEmote>()
            {
                {"SignedUp", Emote.Parse("<:SignedUp:873738981022515261>")},
                {"Absent", Emote.Parse("<:Absent:873737907704320051>")},
                {"Late", Emote.Parse("<:Late:873739705164914758>")},
            };

            var tokens = commandParamaters.CommandWords;
            if (tokens.Length != 4)
            {
                DiscordInterface.SendMessage(
                    "Malformed command. Use the following format: !create <event name> <mon, tue, wed, thur, fri, sat, sun> <hh:mm>");
                return;
            }
            string eventName = tokens[1];

            string day = "";
            switch (tokens[2].ToLower())
            {
                case "mon": day = "Monday"; break;
                case "tue": day = "Tuesday"; break;
                case "wed": day = "Wednesday"; break;
                case "thur": day = "Thursday"; break;
                case "fri": day = "Friday"; break;
                case "sat": day = "Saturday"; break;
                case "sun": day = "Sunday"; break;
                default:
                    DiscordInterface.SendMessage(
                        "Malformed command. Could not interpret day. Use one of the following: mon, tue, wed, thur, fri, sat, or sun.");
                    return;
            }

            //Parse time in HH:MM - both 12 and 24 format. With and without leading zeroes.
            string time = "00:00";
            var rex = new Regex("(^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$)");
            if (rex.IsMatch(tokens[3]))
            {
                time = rex.Match(tokens[3]).Groups[0].ToString();
            }
            else
            {
                DiscordInterface.SendMessage(
                    "Malformed command. Could not interpret time. Specify time in HH:MM format.");
                return;
            }

            var message = DiscordInterface.SendMessage(
                "Sign up for " + eventName + " next " + day + " at " + time + " " + MentionUtils.MentionRole(Config.Read.MentionRoleID) + "!"); //ribbe
            DiscordInterface.SendMessage("RaidID: " + message.Id + " - use !Roster " + message.Id + " to display the roster.");

            message.AddReactionAsync(emotes["SignedUp"]);
            message.AddReactionAsync(emotes["Late"]);
            message.AddReactionAsync(emotes["Absent"]);
        }

        [CallableCommand("roster")]
        public void CommandRoster(CommandParamaters commandParamaters)
        {
            var tokens = commandParamaters.CommandWords;

            if (tokens.Length == 2)
            {
                //All messages that are posted in Discord are tied to a UID.
                //We post that UID once we create a raid (call it the raidID).
                //User can ask for the roster by using !roster raidID.
                //We then fetch all reactions for that message, which is interpreted as the roster.
                //Then post that roster to the Discord channel.

                //Get the raid message for the raidID the user provided.
                ulong raidID = 0;
                if (!ulong.TryParse(tokens[1], out raidID))
                {
                    DiscordInterface.SendMessage("Can't find that raid ID.");
                    return;
                }

                var message = DiscordInterface.Client.GetGuild(Config.Read.GuildID)
                    .GetTextChannel(Config.Read.ChannelID)
                    .GetMessageAsync(raidID).GetAwaiter().GetResult();

                //Produce an output such as the following:

                //Roster: 
                //SignedUp: Player1, Player2
                //Tentative: Player3
                //Absent: Player4, Player5
                string rosterMessageToSend = "Roster: \n";
                foreach (var emote in message.Reactions.Keys)
                {
                    //Get all users that reacted with a given reaction.
                    var users = DiscordInterface.Client.GetGuild(Config.Read.GuildID)
                        .GetTextChannel(Config.Read.ChannelID)
                        .GetMessageAsync(raidID).Result
                        .GetReactionUsersAsync(emote, 25)
                        .ToListAsync().Result;

                    string players = "";
                    foreach (var user in users[0])
                    {
                        if (user.IsBot) continue;
                        players += user.Username + ",";
                    }
                    if (players == "") continue;

                    rosterMessageToSend += emote.Name + ": " + players.Remove(players.Length - 1) + "\n";
                }

                DiscordInterface.SendMessage(rosterMessageToSend);
            }
            else
            {
                DiscordInterface.SendMessage("Malformed command. Use the following format: !roster <RaidID>");
                return;
            }
        }

        [CallableCommand("raiderz")]
        public void CommandRaiderz(CommandParamaters commandParamaters)
        {
            var tokens = commandParamaters.CommandWords;

            if (tokens.Length != 4) { throw new Exception("Incorrect format."); }

            var ge = new GamerEvent();
            ge.Name = tokens[1];
            ge.Time = Util.ParseDate(tokens[2], tokens[3]);
            
            var post = DiscordInterface.SendMessage(ge.ToString());
            ge.MessageID = post.Id;
            
            PermanentStorage.Write(root =>
            {
                root.GamerEvents.Add(ge);
            });

            post.AddReactionAsync(Reactions.SignedUp);
            post.AddReactionAsync(Reactions.Tentative);
            post.AddReactionAsync(Reactions.Declined);
        }

        [CallableCommand("showraidz")]
        public void ShowRaids(CommandParamaters commandParamaters)
        {
            var gamerEvents = PermanentStorage.Read().GamerEvents;
            var sb = new StringBuilder();

            sb.AppendLine("Upcoming raidz:");

            foreach (var gamerEvent in gamerEvents)
            {
                sb.AppendLine(gamerEvent.ToString());
            }

            DiscordInterface.SendMessage(sb.ToString());
        }

        [CallableCommand("clearraidz")]
        public void ClearRaids(CommandParamaters commandParamaters)
        {
            PermanentStorage.Write(root =>
            {
                root.GamerEvents.Clear();
            });
        }
    }

    class CommandParamaters
    {
        public string CompleteCommand { get; }
        public string[] CommandWords { get; }

        public CommandParamaters(string completeCommand)
        {
            CompleteCommand = completeCommand;
            CommandWords = CompleteCommand.Split(' ');
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class CallableCommand : Attribute
    {
        public string Name;

        public CallableCommand(string name)
        {
            Name = name;
        }
    }
}
