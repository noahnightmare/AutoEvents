using AutoEvents.Enums;
using AutoEvents.Models;
using AutoEvents.Interfaces;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Handlers = Exiled.Events.Handlers;
using Object = UnityEngine.Object;

using AutoEvents.Controllers;
using Exiled.API.Features;
using Exiled.API.Enums;
using Exiled.API.Features.Doors;
using Exiled.API.Features.Pickups.Projectiles;
using Exiled.API.Extensions;
using InventorySystem.Items.Pickups;
using Interactables.Interobjects.DoorUtils;
using PlayerRoles.PlayableScps.Scp079.Overcons;
using CustomPlayerEffects;
using Exiled.Events.EventArgs.Player;

namespace AutoEvents.Events.ProtectThePresident
{
    // implement the IHidden interface if you don't want the event to be registered/seen
    public class ProtectThePresident : Event
    {
        // Set the info for the event.
        public override string Name { get; set; } = "ProtectThePresident";
        public override EventType eventType { get; set; } = EventType.Event;
        public override string CommandName { get; set; } = "ptp";

        private Player _winner { get; set; }
        private Side _winnerSide { get; set; }

        // event handlers, unique per plugin
        // register game logic within EventHandler per event
        private EventHandler _handler { get; set; }

        private CoroutineHandle _coroutine { get; set; }

        public readonly Config _config = new Config();

        // events only need registering when the event is being ran
        protected override void RegisterEvents()
        {
            _handler = new EventHandler(_config);
            Handlers.Server.RespawningTeam += _handler.OnRespawningTeam;
            Handlers.Warhead.Starting += _handler.OnWarheadStarting;
            Handlers.Player.PickingUpItem += OnPickingUpItem;
            Handlers.Map.AnnouncingScpTermination += _handler.OnAnnouncingScpTermination;
            Handlers.Player.Shooting += _handler.OnPlayerShooting;
            Handlers.Player.Escaping += OnEscaping;
        }

        // events unregistered when the event finishes
        protected override void UnregisterEvents()
        {
            Handlers.Server.RespawningTeam -= _handler.OnRespawningTeam;
            Handlers.Warhead.Starting -= _handler.OnWarheadStarting;
            Handlers.Player.PickingUpItem -= OnPickingUpItem;
            Handlers.Map.AnnouncingScpTermination -= _handler.OnAnnouncingScpTermination;
            Handlers.Player.Shooting -= _handler.OnPlayerShooting;
            Handlers.Player.Escaping -= OnEscaping;
            _handler = null;
        }

        private bool presidentCanPickup { get; set; } = false;

        // define what happens at the start of the event
        protected override void OnStart()
        {
            _winner = null;
            _winnerSide = Side.None;
            presidentCanPickup = false;

            foreach (Door door in Door.List)
            {
                door.IsOpen = false;
                door.ChangeLock(DoorLockType.AdminCommand);
            }

            foreach(Player player in Player.List.Where(x => !x.IsOverwatchEnabled))
            {
                player.Role.Set(_config.MainRole);
                player.Position = Room.Get(_config.MainRoom).WorldPosition(_config.MainRelativePosition);
            }

            Player president = Player.List.Where(x => x.Role == _config.MainRole).GetRandomValue();
            president.Role.Set(_config.PresidentRole);

            int guardAmount = 0;

            switch (Player.List.Where(x => !x.IsOverwatchEnabled).Count())
            {
                case < 20:
                    guardAmount = 2;
                    break;
                case >= 20 and < 30:
                    guardAmount = 4;
                    break;
                case >= 30 and < 40:
                    guardAmount = 6;
                    break;
                default:
                    guardAmount = 8;
                    break;
            }

            List<Player> presidentGuards = new List<Player>();

            for (int i = 0; i < guardAmount; i++)
            {
                presidentGuards.Add(Player.List.Where(x => x.Role == _config.MainRole).GetRandomValue());
                presidentGuards[i].Role.Set(_config.PresidentGuardRole);
                presidentGuards[i].EnableEffect<DamageReduction>(120, 0);
            }

            foreach (Player player in Player.List.Where(x => x != president))
            {
                player.Position = presidentGuards.Contains(player) ? Room.Get(_config.PresidentRoom).WorldPosition(_config.PresidentRelativePosition) : Room.Get(_config.MainRoom).WorldPosition(_config.MainRelativePosition);
                player.Broadcast(200, presidentGuards.Contains(player) ? "<b><color=#0096FF>You are a Guard of the President.</color>\nEscort the President to the escape to win.\nGet them to the end before you get killed by Chaos!</b>" :
                    "<b><color=#008F1E>You are an Insurgent.</color>\nStop the President from getting to the escape.</b>");
            }

            president.Broadcast(200, "<b><color=#FFFF7C>You are the President.</color>\nYour fellow MTF will escort you to the escape to win.\nGet to the end before you get killed by Chaos!</b>");
            president.Position = Room.Get(_config.PresidentRoom).WorldPosition(_config.PresidentRelativePosition);
            president.EnableEffect<DamageReduction>(120, 0);

            Cassie.MessageTranslated("5 . 4 . 3 . 2 . 1", "5.. 4.. 3.. 2.. 1..", default, false, default);

            Timing.CallDelayed(5f, () =>
            {
                Door.Get(DoorType.Scp173Gate).IsOpen = true;
                Door.Get(DoorType.Intercom).IsOpen = true;
                foreach (Door door in Door.List)
                {
                    door.ChangeLock(DoorLockType.None);
                }
                Map.Broadcast(200, "<b>Protect The President\n<color=#0096FF>NTF</color> & <color=#FFFF7C>President (Scientist)</color> vs <color=#008F1E>Chaos</color>\nEscort/Kill the President to win!</b>");
                foreach(Door door in Door.List.Where(d => d.Type == DoorType.HID || d.Type == DoorType.Scp106Primary || d.Type == DoorType.Scp106Secondary || d.Type == DoorType.Scp914Gate || d.Type == DoorType.Scp330 || d.Type == DoorType.Scp330Chamber))
                {
                    door.ChangeLock(DoorLockType.AdminCommand);
                }
            });
        }

        // Use this method to return a bool to determine if the event should finish
        // If it returns false, the event will continue running through ProcessEventLogic()
        protected override bool IsEventDone()
        {
            if (_winnerSide != Side.None)
            {
                return true;
            }

            if (Player.List.Where(x => x.Role == _config.PresidentRole).IsEmpty() && _winnerSide == Side.None)
            {
                _winnerSide = Side.ChaosInsurgency;
                return true;
            }

            return false;
        }

        // How long between each ProcessEventLogic() is called
        protected override float coroutineDelay { get; set; } = 1f;

        // This method is called once per second while the event is running.
        // Use coroutineDelay to change the delay between each run
        protected override void ProcessEventLogic()
        {
            if (Player.List.Count(x => x.Role == _config.PresidentGuardRole) == 0 && !presidentCanPickup)
            {
                presidentCanPickup = true;
                Player.Get(x => x.Role == _config.PresidentRole).FirstOrDefault().ShowHint("<b><color=red>All of your guards have died!</color>\nYou can now pick up items.\nDefend yourself!</b>");
            }
        }

        // This executes only if the event finishes. If the event is stopped. OnStop will be called instead.
        protected override void OnEnd()
        {
            WinnerController.HandleEventWinner(_winner, _winnerSide, _config.EndMessage);
        }

        // Can be used to broadcast that the event is stopping. Can also be used to stop extra coroutines.
        // NOT NEEDED it's optional
        protected override void OnStop()
        {
            base.OnStop();
        }

        // How long to wait before running cleanups & restarting the round
        protected override float DelayForRestartingTheRound { get; set; } = 20f;


        // Always called after event ends - 20 seconds after finishing the event
        // Here we should cleanup extra gameobjects that we spawn in
        // Ragdolls and items are automatically cleaned up for us

        // However due to the round restarting, this isn't always necessary. It's a nice addition if we plan on using primitives from MER
        protected override void OnCleanup()
        {
           
        }

        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (_config.rolesThatCantPickup.Contains(ev.Player.Role.Type) && !presidentCanPickup)
            {
                ev.IsAllowed = false;
            }
        }

        public void OnEscaping(EscapingEventArgs ev)
        {
            if (ev.Player.Role.Type == _config.PresidentRole)
            {
                _winnerSide = Side.Mtf;
            }
        }
    }
}