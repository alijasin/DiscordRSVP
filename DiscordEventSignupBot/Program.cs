using System.IO;
using Discord.WebSocket;
using Discord;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace NSA_GTFO
{
    class Program
    {
        static DiscordSocketClient Client;

        //Todo: create config file
        static ulong guildID = 130424384614563840; //Discord server ID
        static ulong channelID = 872231780227379251; //For prod: 872231780227379251 and debug: 863015924382040084
        static ulong mentionRoleID = 873598112445386863; //For the people who will be tagged on event creation.

        //Commands
        const string HelpCommand = "!help";
        const string RosterCommand = "!roster";
        const string CreateCommand = "!create";

        static void Main(string[] args)
        {
            System.Threading.Thread t = new System.Threading.Thread( () => 
            { 
                SetBotUp(File.ReadAllText("token.txt")).GetAwaiter().GetResult();

                StartListening();
            });

            t.Start();

            //Keep thread alive
            while(true) System.Threading.Thread.Sleep(100);
        }

        static async Task SetBotUp(string token)
        {
            Client = new DiscordSocketClient();

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            //Give the bot 1 second to connect to Discord
            System.Threading.Thread.Sleep(2500);

            SendMessage("Bot online. Type !help for a list of commands.");
        }

        static void StartListening()
        {
            Client.MessageReceived += async (a) =>  
            {
                //Don't listen to non-command messages nor bot's own messages.
                if(!a.ToString().StartsWith('!') || a.Author.IsBot) 
                {
                    return;
                } 

                ExecuteCommand(a.ToString().ToLower());
            };
        }

        static async void ExecuteCommand(string command)
        {
            //Init the emojis and emotes 
            Dictionary<string, IEmote> emotes = new Dictionary<string, IEmote>()
            {
                {"SignedUp", Emote.Parse("<:SignedUp:873738981022515261>")},
                {"Absent", Emote.Parse("<:Absent:873737907704320051>")},
                {"Late", Emote.Parse("<:Late:873739705164914758>")},
            };

            //Split a command into tokens. 
            //For example: "!kara tue 20:20" -> [!kara, tue, 20:20]
            //Or: !roster 20144124 -> [!roster, 20144124]
            var tokens = command.Split(' ');

            //Get action - either ask for help, pull roster, or create raid.
            string action = tokens[0];
            
            if(action == HelpCommand) //!help
            {
                SendMessage("Available commands: \n" + 
                            "!create <Event name> <mon, tue, wed, thur, fri, sat, sun> <HH:MM> \n " +
                            "!roster <RaidID> \n");
                return;
            }
            else if(action == RosterCommand) //!roster <raidID>
            {
                if(tokens.Length == 2)
                {
                    //All messages that are posted in Discord are tied to a UID.
                    //We post that UID once we create a raid (call it the raidID).
                    //User can ask for the roster by using !roster raidID.
                    //We then fetch all reactions for that message, which is interpreted as the roster.
                    //Then post that roster to the Discord channel.

                    //Get the raid message for the raidID the user provided.
                    ulong raidID = 0;
                    if(!ulong.TryParse(tokens[1], out raidID)) 
                    {
                        SendMessage("Can't find that raid ID.");
                        return;
                    }
                    
                    var message = await Client.GetGuild(guildID).GetTextChannel(channelID).GetMessageAsync(raidID);

                    //Produce an output such as the following:

                    //Roster: 
                    //SignedUp: Player1, Player2
                    //Tentative: Player3
                    //Absent: Player4, Player5
                    string rosterMessageToSend = "Roster: \n";
                    foreach(var emote in message.Reactions.Keys)
                    {
                        //Get all users that reacted with a given reaction.
                        var users = Client.GetGuild(guildID).GetTextChannel(channelID).GetMessageAsync(raidID).Result.GetReactionUsersAsync(emote, 25).ToListAsync().Result;

                        string players = "";
                        foreach(var user in users[0])
                        {
                            if(user.IsBot) continue;
                            players += user.Username + ",";
                        }
                        if(players == "") continue;

                        rosterMessageToSend += emote.Name + ": " + players.Remove(players.Length - 1) + "\n";
                    }

                    SendMessage(rosterMessageToSend);
                }
                else
                {
                    SendMessage("Malformed command. Use the following format: !roster <RaidID>");
                    return;
                }
            }
            else if(action == CreateCommand)
            {
                if(tokens.Length != 4)
                {
                    SendMessage("Malformed command. Use the following format: !create <event name> <mon, tue, wed, thur, fri, sat, sun> <hh:mm>");
                    return;
                }
                string eventName = tokens[1];

                string day = "";
                switch(tokens[2])
                {
                    case "mon": day = "Monday"; break;
                    case "tue": day = "Tuesday"; break;
                    case "wed": day = "Wednesday"; break;
                    case "thur": day = "Thursday"; break;
                    case "fri": day = "Friday"; break;
                    case "sat": day = "Saturday"; break;
                    case "sun": day = "Sunday"; break;
                    default: 
                        var sent = SendMessage("Malformed command. Could not interpret day. Use one of the following: mon, tue, wed, thur, fri, sat, or sun.");
                        return;
                }

                //Parse time in HH:MM - both 12 and 24 format. With and without leading zeroes.
                string time = "00:00";
                Regex rex = new Regex("(^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$)");
                if(rex.IsMatch(tokens[3]))
                {
                    time = rex.Match(tokens[3]).Groups[0].ToString();
                }
                else 
                {
                    SendMessage("Malformed command. Could not interpret time. Specify time in HH:MM format.");
                    return;
                }

                Discord.Rest.RestUserMessage message = SendMessage("Sign up for " + eventName + " next " + day + " at " + time + " " + MentionUtils.MentionRole(mentionRoleID) +"!");
                SendMessage("RaidID: " + message.Id + " - use !Roster " + message.Id + " to display the roster.");
                await message.AddReactionAsync(emotes["SignedUp"]);
                await message.AddReactionAsync(emotes["Late"]);
                await message.AddReactionAsync(emotes["Absent"]);
            }
            else
            {
                SendMessage("Malformed message dog (fukk QA)");
                return;
            }
        }

        static Discord.Rest.RestUserMessage SendMessage(string text)
        {
            return Client.GetGuild(guildID).GetTextChannel(channelID).SendMessageAsync(text).Result;
        }
    }
}