using CommandSystem;
using Exiled.API.Features;
using System;
using Exiled.Permissions.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoEvents.Commands
{
    [CommandHandler(typeof(ClientCommandHandler))]
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class RelativePosCommand : ICommand
    {
        public string Command => "relativepos";

        public string[] Aliases => new string[0];

        public string Description => "Returns relative position of a player.";

        public bool SanitizeResponse => false;

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get(sender);

            if (!player.CheckPermission("autoevents.utils"))
            {
                response = "You can't use this command!";
                return false;
            }

            Vector3 relPos = player.CurrentRoom.Type != Exiled.API.Enums.RoomType.Surface ? player.CurrentRoom.LocalPosition(player.Position) : player.Position;

            Vector3 relRot = player.CurrentRoom.LocalPosition(player.CameraTransform.forward);

            response = $"Relative position: {relPos}\nRotation: {relRot}\nRoomType: {player.CurrentRoom.Type}";
            return true;
        }

        // LocalPosition converts from World to Local (GETTING)
        // WorldPosition converts from Local to World (SETTING)
    }
}
