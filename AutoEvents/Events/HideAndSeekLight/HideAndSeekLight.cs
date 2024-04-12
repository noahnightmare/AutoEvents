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
using CustomPlayerEffects;
using LightContainmentZoneDecontamination;

namespace AutoEvents.Events.HideAndSeekLight
{
    // implement the IHidden interface if you don't want the event to be registered/seen
    public class HideAndSeekLight : Event
    {
        // Set the info for the event.
        public override string Name { get; set; } = "HideAndSeekLight";
        public override EventType eventType { get; set; } = EventType.Event;
        public override string CommandName { get; set; } = "hasl";

        private Player _winner { get; set; } = null;
        private Side _winnerSide { get; set; } = Side.None;

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
            Handlers.Player.PickingUpItem += _handler.OnPickingUpItem;
            Handlers.Map.AnnouncingScpTermination += _handler.OnAnnouncingScpTermination;
            Handlers.Player.PlayerDamageWindow += _handler.OnPlayerDamageWindow;
        }

        // events unregistered when the event finishes
        protected override void UnregisterEvents()
        {
            Handlers.Server.RespawningTeam -= _handler.OnRespawningTeam;
            Handlers.Warhead.Starting -= _handler.OnWarheadStarting;
            Handlers.Player.PickingUpItem -= _handler.OnPickingUpItem;
            Handlers.Map.AnnouncingScpTermination -= _handler.OnAnnouncingScpTermination;
            Handlers.Player.PlayerDamageWindow -= _handler.OnPlayerDamageWindow;
            _handler = null;
        }

        // define what happens at the start of the event
        protected override void OnStart()
        {
            _winner = null;
            _winnerSide = Side.None;

            DecontaminationController.Singleton.DecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled;

            foreach (Player player in Player.List.Where(x => !x.IsOverwatchEnabled))
            {
                player.Role.Set(_config.Role);
                player.Broadcast(30, "<b><color=red>The Seeker will be released in 30 seconds...</color></b>");
                player.Scale = _config.Scale;
            }

            Player randomPlayer = Player.List.Where(x => x.Role == _config.Role).GetRandomValue();
            randomPlayer.Role.Set(_config.SeekerRole);
            randomPlayer.Position = Room.Get(_config.SeekerRoom).WorldPosition(_config.SeekerRelativePosition);

            randomPlayer.EnableEffect<Ensnared>();
            randomPlayer.EnableEffect<MovementBoost>(75, 0);

            Timing.CallDelayed(_config.TimeToLetHidersHide, () =>
            {
                foreach (Player player in Player.List.Where(x => x.Role == _config.SeekerRole || x.Role == _config.deadPlayerRole))
                {
                    player.DisableEffect<Ensnared>();
                }

                Cassie.MessageTranslated("jam_010_2 SCP 0 4 9 pitch_0.9 has breached containment . pitch_0.9 All ClassD Personnel must jam_020_2 pitch_0.7 run pitch_0.8 immediately . ",
                        "<color=red>SCP-049 has breached containment.</color> All ClassD Personnel must run immediately.");
            });

            randomPlayer.ClearBroadcasts();
            randomPlayer.Broadcast((ushort)_config.TimeToLetHidersHide, $"<b>You have been chosen as the starting Seeker!\n<color=red>Kill everyone.\nYou are frozen for {_config.TimeToLetHidersHide} seconds to let people hide.</color></b>");

            Map.Broadcast(200, "<b>Small Hide And Seek [LIGHT]\n<color=orange>Be the last Class D remaining!</color>");

            foreach (Door door in Door.List.Where(d => d.IsCheckpoint || d.IsPartOfCheckpoint || d.Type == DoorType.Scp914Gate || d.Type == DoorType.Scp330 || d.Type == DoorType.Scp330Chamber))
            {
                door.IsOpen = false;
                door.ChangeLock(DoorLockType.AdminCommand);
            }

            foreach(Lift lift in Lift.List)
            {
                lift.ChangeLock(DoorLockReason.AdminCommand);
            }
        }

        // Use this method to return a bool to determine if the event should finish
        // If it returns false, the event will continue running through ProcessEventLogic()
        protected override bool IsEventDone()
        {
            if (Player.List.Count(x => x.Role == _config.Role) <= 1 && _winner == null)
            {
                _winner = Player.List.FirstOrDefault(x => x.Role == _config.Role);
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
            foreach(Player player in Player.List.Where(x => x.Role == RoleTypeId.Spectator))
            {
                player.Role.Set(_config.deadPlayerRole);
                player.Position = Room.Get(_config.SeekerRoom).WorldPosition(_config.SeekerRelativePosition);
                player.Scale = _config.Scale;

                if (EventTime.TotalSeconds <= _config.TimeToLetHidersHide)
                {
                    player.EnableEffect<Ensnared>();
                }
            }
        }

        // This executes only if the event finishes. If the event is stopped. OnStop will be called instead.
        protected override void OnEnd()
        {
            // ALWAYS call this on round end! _winner and _winnerSide can be null/Side.None
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
    }
}