using AutoEvents.Controllers;
using CommandSystem;
using Exiled.API.Features;
using RemoteAdmin;
using Exiled.Permissions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoEvents.Models;
using AutoEvents.Extensions;
using Exiled.API.Enums;

namespace AutoEvents.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    [CommandHandler(typeof(ClientCommandHandler))]
    public class AutoEventCommand : ICommand
    {
        public string Command { get; } = "event";

        public string[] Aliases { get; } = { "e" };

        public string Description { get; } = "event <eventname>";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            Player player = Player.Get((sender as PlayerCommandSender).ReferenceHub);

            if (!player.CheckPermission("autoevents.bypass"))
            {
                if (!player.CheckPermission("autoevents.start"))
                {
                    response = "<color=red>You don't have permissions to use this command.</color>";
                    return false;
                }
            }

            if (AutoEvents.shouldDisallowEventsThisRound)
            {
                response = "The events are disabled for this round";
                return false;
            }

            if (AutoEvents.isEventRunning)
            {
                response = "An event is running right now.";
                return false;
            }

            if (AutoEvents.isEventVoteRunning) 
            {
                response = "An event vote is running right now.";
                return false;
            }

            if (Player.List.Count() < AutoEvents.Instance.Config.MinimumPlayersToRequest)
            {
                response = $"You need {AutoEvents.Instance.Config.MinimumPlayersToRequest} players to request an event.";
                return false;
            }

            if (arguments.Count == 0)
            {
                response = "You need to enter the name of the event.\nUsage: event <eventName>";
                return false;
            }

            if (Event.GetEvent(arguments.At(0)) == null)
            {
                string res = "You need to enter a valid event name.\nEvents:\n";
                foreach (Event ev in Event.Events)
                {
                    res += $"{ev.Name} [{ev.CommandName}]\n";
                }

                response = res;
                return false;
            }

            // experimental zone specifying code

            /* ZoneType zone = ZoneType.Unspecified;

            if (arguments.At(1) != null && (!Enum.TryParse(arguments.At(1), out zone) || !Enum.TryParse(arguments.At(1) + "Containment", out zone)))
            {
                response = "Could not parse second argument as a valid ZoneType. Use either Light, Heavy or Entrance";
                return false;
            }

            if (zone != ZoneType.LightContainment || zone != ZoneType.HeavyContainment || zone != ZoneType.Entrance)
            {
                response = "Invalid zone type.";
                return false;
            } */

            if (player.HasLocalCooldown())
            {
                if (player.LocalCooldown().RemainingCooldownRounds > 0)
                {
                    response = $"You have a local cooldown, you must wait {player.LocalCooldown().RemainingCooldownRounds} rounds more to use this command.";
                    return false;
                }
            }

            if (AutoEvents.Instance.CooldownController._cooldown.GlobalCooldown > 0)
            {
                response = $"The global event cooldown is active, you must wait {AutoEvents.Instance.CooldownController._cooldown.GlobalCooldown} rounds more to use this command.";
                return false;
            }

            if (Round.IsStarted)
            {
                if (!AutoEvents.isEventRunning)
                {
                    if (player.HasLocalCooldown())
                    {
                        player.LocalCooldown().RemainingCooldownRounds = AutoEvents.Instance.Config.LocalCooldown;
                    }
                    else
                    {
                        player.CreateLocalCooldown();
                    }

                    AutoEvents.Instance.CooldownController._cooldown.GlobalCooldown = AutoEvents.Instance.Config.GlobalCooldown;
                    AutoEvents.Instance.CooldownController.QueueEvent(Event.GetEvent(arguments.At(0)), Player.Get(sender));
                    response = "Event will start in the next round.";
                    return true;
                }
                else
                {
                    response = "Someone already requested an event for the next round";
                    return false;
                }
            }

            if (player.HasLocalCooldown())
            {
                player.LocalCooldown().RemainingCooldownRounds = AutoEvents.Instance.Config.LocalCooldown;
            }
            else
            {
                player.CreateLocalCooldown();
            }

            AutoEvents.Instance.CooldownController._cooldown.GlobalCooldown = AutoEvents.Instance.Config.GlobalCooldown;

            new EventController(Event.GetEvent(arguments.At(0)), Player.Get(sender));

            response = "Starting the event...";

            return true;
        }
    }
}
