using AutoEvents.Controllers;
using CommandSystem;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
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

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player p = Player.Get(sender);

            if (WinnerController.winner == null)
            {
                response = "You haven't won an event";
                return false;
            }

            if (!Round.IsStarted)
            {
                response = "You haven't won an event";
                return false;
            }

            if (p != WinnerController.winner)
            {
                response = "You haven't won an event";
                return false;
            }

            if (arguments.Count != 1)
            {
                response = "You need to enter a RoleType\nUsage: .role <RoleType>\nExample: .role Scp173";
                return false;
            }

            RoleTypeId role;

            // Checks all cases
            if (!Enum.TryParse(arguments.At(0), true, out role) && !Enum.TryParse("Scp" + arguments.At(0), true, out role) && !Enum.TryParse("Scp-" + arguments.At(0), true, out role))
            {
                response = "Error parsing the RoleTypeId.\nAll Roles: Scp173, Scp096, Scp939, Scp106, Scp049, Scp079, Scp3114, ClassD, Scientist, FacilityGuard";
                return false;
            }

            if (role == RoleTypeId.Tutorial || role == RoleTypeId.ChaosConscript || role == RoleTypeId.Scp0492 || role == RoleTypeId.ChaosMarauder || role == RoleTypeId.ChaosRepressor || role == RoleTypeId.ChaosRifleman || role == RoleTypeId.NtfCaptain || role == RoleTypeId.NtfPrivate || role == RoleTypeId.NtfSergeant || role == RoleTypeId.NtfSpecialist)
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