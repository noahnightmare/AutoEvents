using CommandSystem;
using System;
using Exiled.Permissions.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]
    public class EventSkipCommand : ICommand
    {
        public string Command { get; } = "eventskip";

        public string[] Aliases { get; } = null;

        public string Description { get; } = "This command prevents people from starting an event.";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (!sender.CheckPermission("autoevents.eventskip"))
            {
                response = "You can't use this command.";
                return false;
            }

            AutoEvents.shouldDisallowEventsThisRound = true;
            response = "Done, now the players can't start an event on this round.";
            return true;
        }
    }
}
