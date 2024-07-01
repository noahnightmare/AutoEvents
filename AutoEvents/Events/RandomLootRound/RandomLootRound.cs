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
using Exiled.Events.EventArgs.Player;
using Exiled.API.Features.Pickups;
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Map;

namespace AutoEvents.Events.RandomLootRound
{
    // implement the IHidden interface if you don't want the event to be registered/seen
    public class RandomLootRound : Event
    {
        // Set the info for the event.
        public override string Name { get; set; } = "RandomLootRound";
        public override EventType eventType { get; set; } = EventType.NormalRound;
        public override string CommandName { get; set; } = "rl";

        private Player _winner { get; set; }
        private Side _winnerSide { get; set; }
        private Player _lastAlive { get; set; }

        private HashSet<ushort> _randomisedPickups = new HashSet<ushort>();

        // event handlers, unique per plugin
        // register game logic within EventHandler per event
        private EventHandler _handler { get; set; }

        public readonly Config _config = new Config();

        private Dictionary<RoleTypeId, int> spawnItemCount = new Dictionary<RoleTypeId, int>()
        {
            { RoleTypeId.ClassD, 2 },
            { RoleTypeId.Scientist, 2 },
            { RoleTypeId.NtfCaptain, 7 },
            { RoleTypeId.NtfSergeant, 6 },
            { RoleTypeId.NtfSpecialist, 6 },
            { RoleTypeId.NtfPrivate, 5 },
            { RoleTypeId.FacilityGuard, 6 },
            { RoleTypeId.ChaosRepressor, 5 },
            { RoleTypeId.ChaosMarauder, 6 },
            { RoleTypeId.ChaosConscript, 5 },
            { RoleTypeId.ChaosRifleman, 5 },
        };

        // events only need registering when the event is being ran
        protected override void RegisterEvents()
        {
            _handler = new EventHandler(_config);
            Handlers.Map.PickupAdded += OnPickupSpawned;
            Handlers.Player.Spawned += OnPlayerSpawned;
            Handlers.Player.PickingUpItem += OnPickingUpItem;
        }

        // events unregistered when the event finishes
        protected override void UnregisterEvents()
        {
            Handlers.Map.PickupAdded -= OnPickupSpawned;
            Handlers.Player.Spawned -= OnPlayerSpawned;
            Handlers.Player.PickingUpItem -= OnPickingUpItem;
            _handler = null;
        }

        // define what happens at the start of the event
        protected override void OnStart()
        {
            _randomisedPickups?.Clear();
            Map.Broadcast(600, "<b>Random Loot Round\n<color=red>Your inventory, and items around the map, are randomized.</color></b>");

            Timing.CallDelayed(0.1f, () => 
            {
                foreach (Player player in Player.List)
                {
                    if (spawnItemCount.ContainsKey(player.Role.Type))
                    {
                        player.ClearInventory();
                        for (int i = 0; i < spawnItemCount[player.Role.Type]; i++)
                        {
                            Item item = player.AddItem(EnumUtils<ItemType>.Values.GetRandomValue(i => i != ItemType.None && !i.IsAmmo()));
                            _randomisedPickups.Add(item.Serial);
                        }
                    }
                }
            });
        }

        // Use this method to return a bool to determine if the event should finish
        // If it returns false, the event will continue running through ProcessEventLogic()
        protected override bool IsEventDone()
        {
            if (Round.IsEnded) return true;

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

        // handles dropped items
        public void OnPickupSpawned(PickupAddedEventArgs ev)
        {
            if (!_randomisedPickups.Contains(ev.Pickup.Serial) && !ev.Pickup.Type.IsAmmo())
            {
                Vector3 pickupPos = ev.Pickup.Position;
                ev.Pickup.Destroy();
                Pickup p = Pickup.CreateAndSpawn(EnumUtils<ItemType>.Values.GetRandomValue(i => i != ItemType.None && !i.IsAmmo()), pickupPos, default);
                _randomisedPickups.Add(p.Serial);
            }
        }

        public void OnPlayerSpawned(SpawnedEventArgs ev)
        {
            if (spawnItemCount.ContainsKey(ev.Player.Role.Type))
            {
                ev.Player.ClearInventory();
                for (int i = 0; i < spawnItemCount[ev.Player.Role.Type]; i++)
                {
                    Item item = ev.Player.AddItem(EnumUtils<ItemType>.Values.GetRandomValue(i => i != ItemType.None && !i.IsAmmo()));
                    _randomisedPickups.Add(item.Serial);
                }
            }
        }

        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (_randomisedPickups.Contains(ev.Pickup.Serial))
            {
                ev.Player.ShowHint("This item has already been randomized!");
            }
            else
            {
                ev.Player.ShowHint("Drop this item to randomize it!");
            }
        }
    }
}