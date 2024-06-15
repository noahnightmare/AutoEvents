using CommandSystem;
using System;
using Exiled.Permissions.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

namespace AutoEvents.Commands
{
    [CommandHandler(typeof(GameConsoleCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class TpsCommand : ICommand
    {
        public string Command { get; } = "tps";

        public string[] Aliases { get; } = null;

        public string Description { get; } = "Checks the TPS of the server";

        public bool SanitizeResponse => false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            response = $"{(int)Server.Tps}/60";
            return true;
        }
    }
}
