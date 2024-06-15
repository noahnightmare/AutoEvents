using AutoEvents.Controllers;
using CommandSystem;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using InventorySystem.Items.Firearms;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Commands 
{ 
    [CommandHandler(typeof(ClientCommandHandler))]
    public class RoleCommand : ICommand
    {
        public string Command => "role";

        public string[] Aliases => new string[0];

        public string Description => "Select the role to play as in the next round if you won the event.";

        public bool SanitizeResponse => false;

        public static List<RoleTypeId> WhitelistedRoles = new List<RoleTypeId>()
        {
            RoleTypeId.ClassD,
            RoleTypeId.Scientist,
            RoleTypeId.FacilityGuard,
            RoleTypeId.Scp173,
            RoleTypeId.Scp096,
            RoleTypeId.Scp939,
            RoleTypeId.Scp049,
            RoleTypeId.Scp106,
            RoleTypeId.Scp079,
            RoleTypeId.Scp3114
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player p = Player.Get(sender);

            if (WinnerController.winner == null)
            {
                response = "You haven't won an event (1)";
                return false;
            }

            if (!WinnerController.canUseRoleCommand)
            {
                response = "You haven't won an event (2)";
                return false;
            }

            if (p.UserId != WinnerController.winner.UserId)
            {
                response = "You haven't won an event (3)";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "You need to enter a RoleType\nUsage: .role <RoleType>\nExample: .role Scp173";
                return false;
            }

            RoleTypeId role;

            // Checks all cases

            if (!Enum.TryParse("Scp" + arguments.At(0), true, out role) && !Enum.TryParse(arguments.At(0), true, out role))
            {
                response = "Error parsing the RoleTypeId.\nAll Roles: Scp173, Scp096, Scp939, Scp106, Scp049, Scp079, Scp3114, ClassD, Scientist, FacilityGuard";
                return false;
            }

            if (!WhitelistedRoles.Contains(role))
            {
                response = "You can't select this role.\nAll Roles: Scp173, Scp096, Scp939, Scp106, Scp049, Scp079, Scp3114, ClassD, Scientist, FacilityGuard";
                return false;
            }

            WinnerController.winnerDesiredRole = role;

            response = $"Done, role selected.";
            return true;
        }
    }
}