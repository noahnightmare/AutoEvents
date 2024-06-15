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
using LightContainmentZoneDecontamination;
using CommandSystem.Commands.RemoteAdmin;

namespace AutoEvents.Events.ZombieEscape
{
    // implement the IHidden interface if you don't want the event to be registered/seen
    public class ZombieEscape : Event
    {
        // Set the info for the event.
        public override string Name { get; set; } = "ZombieEscape";
        public override EventType eventType { get; set; } = EventType.Event;
        public override string CommandName { get; set; } = "ze";

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
            Handlers.Map.AnnouncingScpTermination += _handler.OnAnnouncingScpTermination;
            Handlers.Player.Shooting += _handler.OnPlayerShooting;
            Handlers.Player.Escaping += OnEscaping;
            Handlers.Player.SpawningRagdoll += _handler.OnPlayerSpawningRagdoll;
            Handlers.Player.Spawned += OnSpawned;
        }

        // events unregistered when the event finishes
        protected override void UnregisterEvents()
        {
            Handlers.Server.RespawningTeam -= _handler.OnRespawningTeam;
            Handlers.Warhead.Starting -= _handler.OnWarheadStarting;
            Handlers.Map.AnnouncingScpTermination -= _handler.OnAnnouncingScpTermination;
            Handlers.Player.Shooting -= _handler.OnPlayerShooting;
            Handlers.Player.Escaping -= OnEscaping;
            Handlers.Player.SpawningRagdoll -= _handler.OnPlayerSpawningRagdoll;
            Handlers.Player.Spawned -= OnSpawned;
            _handler = null;
        }

        private List<Player> Zombies { get; set; } = new List<Player>();
        private ZoneType currentZone { get; set; } = ZoneType.LightContainment;
        private float currentZombieHealth { get; set; } = 400f;
        private float currentZombieSpeed { get; set; } = 0f;
        private RoomType currentZombieRoom { get; set; } = RoomType.Lcz173;
        private float currentZombieDamage { get; set; } = 20f;

        private Vector3 currentZombieRelativePosition { get; set; } = Vector3.zero;

        // define what happens at the start of the event
        protected override void OnStart()
        {
            _winner = null;
            _winnerSide = Side.None;

            DecontaminationController.Singleton.DecontaminationOverride = DecontaminationController.DecontaminationStatus.Disabled;

            foreach(Door door in Door.List.Where(d => d.IsCheckpoint || d.IsPartOfCheckpoint || d.Type == DoorType.Scp914Gate || d.Type == DoorType.Scp330 || d.Type == DoorType.Scp330Chamber || d.Type == DoorType.LczArmory ||
                d.Type == DoorType.GateA || d.Type == DoorType.GateB)) 
            {
                door.IsOpen = false;
                door.ChangeLock(DoorLockType.AdminCommand);
            }

            Door.Get(DoorType.Intercom).IsOpen = true;
            Door.Get(DoorType.Scp079First).IsOpen = true;

            foreach (Player player in Player.List.Where(x => !x.IsOverwatchEnabled))
            {
                player.Role.Set(_config.Role);
                player.ClearInventory();
                player.AddItem(_config.FirstInventory);
                player.Broadcast(200, "<b><color=red>You are a survivor.</color>\nMake your way through the facility, and escape to win!\nHeavy will open in 2 minutes.</b>");
            }

            int zombieAmount = 0;

            switch (Player.List.Where(x => !x.IsOverwatchEnabled).Count())
            {
                case < 20:
                    zombieAmount = 2;
                    break;
                case >= 20 and < 30:
                    zombieAmount = 4;
                    break;
                case >= 30 and < 40:
                    zombieAmount = 6;
                    break;
                default:
                    zombieAmount = 8;
                    break;
            }

            for (int i = 0; i < zombieAmount; i++)
            {
                Zombies.Add(Player.List.Where(x => x.Role == _config.Role).GetRandomValue());
                Zombies[i].Role.Set(_config.ZombieRole);
                Zombies[i].IsGodModeEnabled = true;
                Zombies[i].Broadcast(200, $"<b>You have been chosen as one of the starting Zombies!\n<color=red>Kill everyone.</color>\nYou have Godmode for the first {_config.ZombieGodmodeTime} seconds!</b>");
                Zombies[i].Position = Room.Get(_config.FirstZombieRoom).WorldPosition(_config.FirstZombieRelativePosition);
            }

            Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . An Infection has been jam_043_2 spotted in the facility . . All ClassD Personnel are required to evacuate to their nearest jam_043_5 Checkpoint immediately .",
                "<color=red>Alert</color> : An infection has been spotted in the facility. All ClassD Personnel are required to evacuate to their nearest Checkpoint immediately.", isNoisy: false);
        }

        // Use this method to return a bool to determine if the event should finish
        // If it returns false, the event will continue running through ProcessEventLogic()
        protected override bool IsEventDone()
        {
            if (Player.List.Count(x => x.Role == _config.Role) == 0)
            {
                _winnerSide = Side.Scp;
                return true;
            }

            if (_winner != null)
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
            foreach (Player player in Player.List.Where(x => x.Role == RoleTypeId.Spectator))
            {
                if (!Zombies.Contains(player))
                {
                    Zombies.Add(player);
                }

                player.Role.Set(_config.ZombieRole);
            }

            foreach(Player player in Player.List.Where(x => x.IsAlive))
            {
                if ((player.Zone & currentZone) == 0)
                {
                    player.Kill(DamageType.Decontamination);
                }
            }

            switch (currentZone)
            {
                case ZoneType.HeavyContainment:
                    currentZombieHealth = _config.SecondZombieHealth;
                    currentZombieSpeed = _config.SecondZombieSpeed;
                    currentZombieDamage = _config.SecondZombieDamage;
                    break;
                case ZoneType.Entrance:
                    currentZombieHealth = _config.ThirdZombieHealth;
                    currentZombieSpeed = _config.ThirdZombieSpeed;
                    currentZombieDamage = _config.ThirdZombieDamage; 
                    break;
                case ZoneType.Other:
                    currentZombieHealth = _config.FourthZombieHealth;
                    currentZombieSpeed = _config.FourthZombieSpeed;
                    currentZombieDamage = _config.FourthZombieDamage;
                    break;
                default:
                    currentZombieRoom = _config.FirstZombieRoom;
                    currentZombieRelativePosition = _config.FirstZombieRelativePosition;
                    currentZombieDamage = 20f;
                    break;
            }

            if (EventTime.TotalSeconds == _config.ZombieGodmodeTime)
            {
                foreach (Player player in Zombies)
                {
                    player.IsGodModeEnabled = false;
                }
            }

            if (EventTime.TotalSeconds == 90f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . pitch_0.9 Light Containment jam_043_3 Zone Checkpoints . will open in T Minus 20 Seconds . . Please evacuate immediately",
                    "<color=red>Alert</color> : <color=yellow>Light Containment Zone Checkpoints will open in T-20 Seconds. Please evacuate immediately.</color>", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 110f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . Any ClassD remaining in Light Containment Zone will die in T Minus 60 seconds",
                    "<color=red>Alert</color> : Any ClassD remaining in Light Containment Zone will die in T-60 seconds.", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 130f)
            {
                Door.Get(DoorType.CheckpointLczA).IsOpen = true;
                Door.Get(DoorType.CheckpointLczA).ChangeLock(DoorLockType.Warhead);

                Door.Get(DoorType.CheckpointLczB).IsOpen = true;
                Door.Get(DoorType.CheckpointLczB).ChangeLock(DoorLockType.Warhead);

                currentZone |= ZoneType.HeavyContainment; // add heavy as valid zone
            }

            if (EventTime.TotalSeconds == 140f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . Any ClassD remaining in Light Containment Zone will die in T Minus 30 seconds",
                    "<color=red>Alert</color> : Any ClassD remaining in Light Containment zone will die in T-30 seconds.", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 180f)
            {
                foreach(Lift lift in Lift.List.Where(l => l.Type == ElevatorType.LczB || l.Type == ElevatorType.LczA)) 
                {
                    lift.ChangeLock(DoorLockReason.AdminCommand);
                }

                currentZone &= ~ZoneType.LightContainment; // remove light as valid zone

                Map.ClearBroadcasts();
                Map.Broadcast(200, "<b><color=yellow>Zombies now respawn in Heavy.</color>\nZombies now have <color=red>750 HP</color> & move faster.\nPlayers now have <color=green>Crossvecs</color>!</b>");

                foreach(Player player in Player.List.Where(x => x.Role == _config.Role))
                {
                    player.ClearInventory();
                    player.AddItem(_config.SecondInventory);
                }

                foreach(Player player in Zombies)
                {
                    player.Position = Room.Get(_config.SecondZombieRoom).WorldPosition(_config.SecondZombieRelativePosition);
                }
            }

            if (EventTime.TotalSeconds == 330f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . pitch_0.9 Heavy Containment jam_043_3 Zone Checkpoints . will open in T Minus 20 Seconds . . Please evacuate immediately",
                    "<color=red>Alert</color> : <color=yellow> Heavy Containment Zone Checkpoints will open in T-10 seconds. Please evacuate immediately.</color>", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 350f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . Any ClassD remaining in Heavy Containment Zone will die in T Minus 60 seconds",
                    "<color=red>Alert</color> : Any ClassD remaining in Heavy Containment Zone will die in T-60 seconds", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 370f)
            {
                Door.Get(DoorType.CheckpointEzHczA).IsOpen = true;
                Door.Get(DoorType.CheckpointEzHczA).ChangeLock(DoorLockType.Warhead);

                Door.Get(DoorType.CheckpointEzHczB).IsOpen = true;
                Door.Get(DoorType.CheckpointEzHczB).ChangeLock(DoorLockType.Warhead);

                currentZone |= ZoneType.Entrance;
            }

            if (EventTime.TotalSeconds == 375f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . Any ClassD remaining in Heavy Containment Zone will die in T Minus 30 seconds",
                    "<color=red>Alert</color> : Any ClassD remaining in Heavy Containment Zone will die in T-30 seconds.", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 410f)
            {
                Door.Get(DoorType.CheckpointEzHczA).IsOpen = false;
                Door.Get(DoorType.CheckpointEzHczA).ChangeLock(DoorLockType.AdminCommand);

                Door.Get(DoorType.CheckpointEzHczB).IsOpen = false;
                Door.Get(DoorType.CheckpointEzHczB).ChangeLock(DoorLockType.AdminCommand);

                currentZone &= ~ZoneType.HeavyContainment;

                Map.ClearBroadcasts();
                Map.Broadcast(200, "<b><color=yellow>Zombies now respawn in Entrance.</color>\nZombies now have <color=red>1000 HP</color>, <color=red>40 damage</color> & move faster.\nPlayers now have <color=green>E-11s</color>!</b>");

                foreach (Player player in Player.List.Where(x => x.Role == _config.Role))
                {
                    player.ClearInventory();
                    player.AddItem(_config.ThirdInventory);
                }

                foreach (Player player in Zombies)
                {
                    player.Position = Room.Get(_config.ThirdZombieRoom).WorldPosition(_config.ThirdZombieRelativePosition);
                }
            }

            if (EventTime.TotalSeconds == 580f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 Alert . . pitch_0.9 Entrance jam_043_3 Zone Gates . will open in T Minus 10 Seconds . . Please evacuate immediately",
                    "<color=red>Alert</color> : <color=yellow>Entrance Zone Gates will open in T-10 seconds. Please evacuate immediately.", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 600f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 <color=red> Alert . . </color> Any ClassD remaining in Entrance Zone will die in T Minus 60 seconds",
                    "<color=red>Alert</color> : Any ClassD remaining in Entrance Zone will die in T-60 seconds", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 610f)
            {
                Door.Get(DoorType.GateA).IsOpen = true;
                Door.Get(DoorType.GateB).IsOpen = true;

                currentZone |= ZoneType.Other;
            }

            if (EventTime.TotalSeconds == 615f)
            {
                Cassie.MessageTranslated("pitch_0.2 .g4 .g4 pitch_0.9 <color=red> Alert . . </color> Any ClassD remaining in Entrance Zone will die in T Minus 30 seconds",
                    "<color=red>Alert<color> : Any ClassD remaining in Entrance Zone will die in T-30 seconds.", isNoisy: false);
            }

            if (EventTime.TotalSeconds == 650f)
            {
                Door.Get(DoorType.GateA).IsOpen = false;
                Door.Get(DoorType.GateA).ChangeLock(DoorLockType.AdminCommand);

                Door.Get(DoorType.GateB).IsOpen = false;
                Door.Get(DoorType.GateB).ChangeLock(DoorLockType.AdminCommand);

                currentZone &= ~ZoneType.Entrance;

                Map.ClearBroadcasts();
                Map.Broadcast(200, "<b><color=yellow>First person to get to the escape wins!!</color>\nZombies now have <color=red>1500 HP</color>, <color=red>50 damage</color> & move faster.\nPlayers now have <color=green>Captain Guns & Railguns</color>!</b>");

                foreach (Player player in Player.List.Where(x => x.Role == _config.Role))
                {
                    player.ClearInventory();
                    player.AddItem(_config.FourthInventory);
                }

                foreach (Player player in Zombies)
                {
                    player.Position = Room.Get(_config.FourthZombieRoom).WorldPosition(_config.FourthZombieRelativePosition);
                }
            }
        }

        // This executes only if the event finishes. If the event is stopped. OnStop will be called instead.
        protected override void OnEnd()
        {
            if (_winner == null)
            {
                // ALWAYS call this on round end! _winner and _winnerSide can be null/Side.None
                WinnerController.HandleEventWinner(_winner, _winnerSide, _config.AlternativeEndMessage);
            }
            else
            {
                WinnerController.HandleEventWinner(_winner, _winnerSide, _config.EndMessage);
            }
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

        public void OnEscaping(EscapingEventArgs ev)
        {
            if (_winner == null)
            {
                _winner = ev.Player;
            }
        }

        public void OnSpawned(SpawnedEventArgs ev)
        {
            if (Zombies.Contains(ev.Player))
            {
                ev.Player.Health = currentZombieHealth;
                ev.Player.EnableEffect<MovementBoost>((byte)currentZombieSpeed, 0);
                ev.Player.Position = Room.Get(currentZombieRoom).WorldPosition(currentZombieRelativePosition);
            }
        }
    }
}