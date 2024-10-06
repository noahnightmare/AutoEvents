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
using Exiled.API.Features.Items;
using Exiled.Events.EventArgs.Player;
using InventorySystem.Items.Usables.Scp330;

namespace AutoEvents.Events.CandyMania
{
    // implement the IHidden interface if you don't want the event to be registered/seen
    public class CandyMania : Event
    {
        // Set the info for the event.
        public override string Name { get; set; } = "CandyMania";
        public override EventType eventType { get; set; } = EventType.NormalRound;
        public override string CommandName { get; set; } = "cm";

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

            Handlers.Player.Dying += _handler.OnDying;
        }

        // events unregistered when the event finishes
        protected override void UnregisterEvents()
        {
            Handlers.Player.Dying -= _handler.OnDying;

            _handler = null;
        }

        // define what happens at the start of the event
        protected override void OnStart()
        {
            _winner = null;
            _winnerSide = Side.None;

            _coroutine = Timing.RunCoroutine(CandyGiver(), "Candy Giver");

            Map.Broadcast(600, "<b>Candy Mania\n<color=red>You spawn with 2 candies. Dying drops even more candy.\nEvery 60 seconds, you get another candy.</color></b>");

            foreach (Player player in Player.List.Where(p => CanAddCandy(p)))
            {
                Timing.CallDelayed(0.5f, () => { player.TryAddCandy(GetRandomCandyID()); });
                Timing.CallDelayed(1f, () => { player.TryAddCandy(GetRandomCandyID()); });
            }
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
            Timing.KillCoroutines(new CoroutineHandle[] { _coroutine });
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

        private IEnumerator<float> CandyGiver()
        {
            // give a new candy to everyone every 60 sec
            for (; ; )
            {
                yield return Timing.WaitForSeconds(_config.CandyTimer);
                foreach (Player player in Player.List.Where(p => CanAddCandy(p)))
                {
                    player.TryAddCandy(GetRandomCandyID());
                    player.ShowHint("You received a candy!", 5);
                }
            }
        }

        private bool CanAddCandy(Player player)
        {
            return player.IsAlive && !player.IsScp && !player.IsInventoryFull;
        }

        private CandyKindID GetRandomCandyID()
        {
            Array values = Enum.GetValues(typeof(CandyKindID));

            CandyKindID candy = (CandyKindID)values.GetValue(UnityEngine.Random.Range(0, values.Length));

            while (candy == CandyKindID.None || candy == CandyKindID.Pink)
            {
                candy = (CandyKindID)values.GetValue(UnityEngine.Random.Range(0, values.Length));
            }

            if (UnityEngine.Random.Range(1, 100) <= _config.PinkCandyChance)
            {
                candy = CandyKindID.Pink;
            }

            return candy;
        }
    }
}