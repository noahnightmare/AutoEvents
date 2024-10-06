using AutoEvents.Models;
using Exiled.API.Features;
using Exiled.API.Features.Items;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoEvents.Events.CandyMania
{
    public class EventHandler
    {
        private readonly Config _config;
        private CoroutineHandle _candyCoroutine;

        public EventHandler(Config config)
        {
            _config = config;
        }

        public void OnDying(DyingEventArgs ev)
        {
            if (ev.Player == null) return;

            for (int i = 0; i < _config.CandyDrops; i++)
            {
                Scp330 candy = (Scp330)Item.Create(ItemType.SCP330);
                
                if (UnityEngine.Random.Range(1, 100) <= _config.PinkCandyChance)
                {
                    candy.RemoveAllCandy();
                    candy.AddCandy(InventorySystem.Items.Usables.Scp330.CandyKindID.Pink);
                }

                candy.ExposedType = candy.Candies.ElementAt(0);
                // candy.AddCandy(Scp330Candies.GetRandom());

                candy.CreatePickup(ev.Player.Position);
            }

            /* Scp330Pickup pickup = Pickup.Create(ItemType.SCP330).As<Scp330Pickup>();
            pickup.ExposedCandy = pickup.Candies[0];
            pickup.Position = ev.Player.Position; */
        }
    }
}
