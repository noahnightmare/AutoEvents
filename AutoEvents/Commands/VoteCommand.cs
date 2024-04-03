using AutoEvents.Controllers;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    public class VoteCommand : ICommand
    {
        public string Command => "vote";

        public string[] Aliases => new string[0];

        public string Description => "Vote for an event on a voting round.";

        public static Dictionary<Player, int> playerVoted= new Dictionary<Player, int>();
        private int _voteAmount = 1;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // dev tool
            if (sender.CheckPermission("autoevents.utils"))
            {
                if (arguments.Count > 0)
                {
                    if (arguments.At(0) == "start")
                    {
                        Log.Info($"Event voting started!");
                        new EventVoteController();
                        response = "Voting started. THIS COMMAND SHOULD ONLY BE USED BY NOAH";
                        return true;
                    }
                }
            }

            if (!AutoEvents.isEventVoteRunning)
            {
                response = "An event vote is not running right now!";
                return false;
            }

            if (arguments.Count == 0 || arguments.Count > 1)
            {
                response = "Invalid use. Correct syntax: .vote <number>\ne.g. .vote 1";
                return false;
            }

            string args = arguments.At(0);

            // handles current voting
            switch (args)
            {
                case "1":
                case "2":
                case "3":
                    EventVoteController.SetVoteEventVotes(int.Parse(args), _voteAmount);
                    break;
                case "4":
                    EventVoteController.SetCancelVotes(_voteAmount);
                    break;
                default:
                    response = "Invalid input. Please choose either 1, 2, 3 or 4 depending on which event you want.";
                    return false;
            }

            // gets current voted event
            int currentEventVotedFor = int.Parse(arguments.At(0));

            // handles vote switching
            if (playerVoted.TryGetValue(Player.Get(sender), out int previousEventVotedFor))
            {
                if (previousEventVotedFor == 4 && currentEventVotedFor != previousEventVotedFor)
                {
                    EventVoteController.SetCancelVotes(-_voteAmount);
                    playerVoted[Player.Get(sender)] = currentEventVotedFor;
                }
                else if (currentEventVotedFor != previousEventVotedFor)
                {
                    EventVoteController.SetVoteEventVotes(--previousEventVotedFor, -_voteAmount);
                }
            }

            // overwrites dictionary with current voted event
            playerVoted[Player.Get(sender)] = currentEventVotedFor;

            response = $"Voted for Option " + int.Parse(arguments.At(0));
            return true;
        }
    }
}
