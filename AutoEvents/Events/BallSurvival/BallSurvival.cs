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

namespace AutoEvents.Events.BallSurvival
{
    // implement the IHidden interface if you don't want the event to be registered/seen
    public class BallSurvival : Event
    {
        // Set the info for the event.
        public override string Name { get; set; } = "BallSurvival";
        public override EventType eventType { get; set; } = EventType.Event;
        public override string CommandName { get; set; } = "bs";

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
            Handlers.Player.PickingUpItem += _handler.OnPickingUpItem;
        }

        // events unregistered when the event finishes
        protected override void UnregisterEvents()
        {
            Handlers.Server.RespawningTeam -= _handler.OnRespawningTeam;
            Handlers.Warhead.Starting -= _handler.OnWarheadStarting;
            Handlers.Player.PickingUpItem -= _handler.OnPickingUpItem;
            _handler = null;
        }

        // define what happens at the start of the event
        protected override void OnStart()
        {
            Map.Broadcast(200, "<b>Ball Survival\nSCP-018 will drop every <color=red>20 seconds</color> on a random target!</b>");
            foreach(Player player in Player.List)
            {
                player.Role.Set(_config.Role);
                player.Health = _config.Health;
                player.Position = Room.Get(_config.Room).WorldPosition(_config.RelativePosition);
            }

            foreach(Door door in Door.List)
            {
                door.IsOpen = false;
                door.ChangeLock(DoorLockType.AdminCommand);
            }

            foreach(Lift lift in Lift.List)
            {
                lift.ChangeLock(DoorLockReason.AdminCommand);
            }

            _coroutine = Timing.RunCoroutine(SpawnBall().CancelWith(() => _winner != null), "Spawn Ball");
        }

        // Use this method to return a bool to determine if the event should finish
        // If it returns false, the event will continue running through ProcessEventLogic()
        protected override bool IsEventDone()
        {
            if (Player.List.Count(x => x.Role <= _config.Role) <= 1 && _winner == null)
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
            
        }

        // This executes only if the event finishes. If the event is stopped. OnStop will be called instead.
        protected override void OnEnd()
        {
            // ALWAYS call this on round end! _winner and _winnerSide can be null/Side.None
            WinnerController.HandleEventWinner(_winner, _winnerSide, _config.EndMessage);

            Timing.KillCoroutines(new CoroutineHandle[] { _coroutine });

            foreach (var item in Object.FindObjectsOfType<ItemPickupBase>())
            {
                GameObject.Destroy(item.gameObject);
            }
        }

        // Can be used to broadcast that the event is stopping. Can also be used to stop extra coroutines.
        // NOT NEEDED it's optional
        protected override void OnStop()
        {
            Timing.KillCoroutines(new CoroutineHandle[] { _coroutine });
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

        private IEnumerator<float> SpawnBall()
        {
            for (; ; )
            {
                Player randomPlayer = Player.List.Where(x => x.Role == _config.Role).GetRandomValue();

                if (randomPlayer == null)
                {
                    yield break;
                }

                Projectile.CreateAndSpawn(ItemType.SCP018, randomPlayer.Position, default, randomPlayer).GameObject.GetComponent<Rigidbody>().AddForce(new Vector3(500, 500, 500));
                yield return Timing.WaitForSeconds(20f);
            }
        }
    }
}