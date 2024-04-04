﻿using AutoEvents.Enums;
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
using Exiled.Events.EventArgs.Player;

namespace AutoEvents.Events.NukeRun
{
    // implement the IHidden interface if you don't want the event to be registered/seen
    public class NukeRun : Event
    {
        // Set the info for the event.
        public override string Name { get; set; } = "NukeRun";
        public override EventType eventType { get; set; } = EventType.Event;
        public override string CommandName { get; set; } = "nr";

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
            Handlers.Player.Escaping += OnEscaping;
        }

        // events unregistered when the event finishes
        protected override void UnregisterEvents()
        {
            Handlers.Server.RespawningTeam -= _handler.OnRespawningTeam;
            Handlers.Player.Escaping -= OnEscaping;
            _handler = null;
        }

        // define what happens at the start of the event
        protected override void OnStart()
        {
            _winner = null;
            _winnerSide = Side.None;

            DecontaminationController.Singleton.DecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled;

            foreach (Player player in Player.List)
            {
                player.Role.Set(_config.Role);
                player.AddItem(ItemType.SCP207, _config.AmountOf207ToGive);
            }

            Map.Broadcast(200, "<b>Nuke Run\n<color=red>You get 4 Colas.</color>\nBe the first to escape in an exploding facility!</b>");
           
            foreach (Door door in Door.List.Where(d => d.Type != DoorType.PrisonDoor))
            {
                door.IsOpen = false;
                door.ChangeLock(DoorLockType.AdminCommand);
            }

            Cassie.MessageTranslated("5 . 4 . 3 . 2 . 1", "Countdown: 5... 4...3...2...1...", default, false, default);

            Timing.CallDelayed(6f, () =>
            {
                foreach (Door door in Door.List)
                {
                    door.ChangeLock(DoorLockType.None);
                }

                Warhead.Start();
                Warhead.IsLocked = true;
            });
        }

        // Use this method to return a bool to determine if the event should finish
        // If it returns false, the event will continue running through ProcessEventLogic()
        protected override bool IsEventDone()
        {
            if (_winner != null)
            {
                return true;
            }

            if (Player.List.Count(x => x.IsAlive) == 0)
            {
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

        private void OnEscaping(EscapingEventArgs ev)
        {
            if (_winner == null)
            {
                _winner = ev.Player;
            }
        }
    }
}