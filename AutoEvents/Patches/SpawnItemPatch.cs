using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Extensions;
using Exiled.API.Features.Items;
using HarmonyLib;
using Interactables.Interobjects.DoorUtils;
using InventorySystem.Items;
using InventorySystem;
using InventorySystem.Items.Pickups;
using MapGeneration.Distributors;
using NorthwoodLib.Pools;
using PluginAPI.Events;
using UnityEngine;

namespace AutoEvents.Patches
{
    [HarmonyPatch(typeof(Locker), nameof(Locker.FillChamber))]
    public class LockerFillChamberPatch
    {
        public static bool Prefix(Locker __instance, LockerChamber ch)
        {
            List<int> list = ListPool<int>.Shared.Rent();
            for (int i = 0; i < __instance.Loot.Length; i++)
            {
                if (__instance.Loot[i].RemainingUses > 0 && (ch.AcceptableItems.Length == 0 || ch.AcceptableItems.Contains(__instance.Loot[i].TargetItem)))
                {
                    for (int j = 0; j <= __instance.Loot[i].ProbabilityPoints; j++)
                    {
                        list.Add(i);
                    }
                }
            }
            if (list.Count > 0)
            {
                int num = list[UnityEngine.Random.Range(0, list.Count)];
                ch.SpawnItem(EnumUtils<ItemType>.Values.GetRandomValue(i => i != ItemType.None), UnityEngine.Random.Range(__instance.Loot[num].MinPerChamber, __instance.Loot[num].MaxPerChamber + 1));
                __instance.Loot[num].RemainingUses--;
            }
            ListPool<int>.Shared.Return(list);

            return false;
        }
    }

    [HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.PlaceItem))]
    public class PlaceItemPatch
    {
        public static bool Prefix(ItemDistributor __instance, SpawnableItem item)
        {
            float num = UnityEngine.Random.Range(item.MinimalAmount, item.MaxAmount);
            List<ItemSpawnpoint> list = ListPool<ItemSpawnpoint>.Shared.Rent();
            foreach (ItemSpawnpoint itemSpawnpoint in ItemSpawnpoint.RandomInstances)
            {
                if (item.RoomNames.Contains(itemSpawnpoint.RoomName) && itemSpawnpoint.CanSpawn(item.PossibleSpawns))
                {
                    list.Add(itemSpawnpoint);
                }
            }
            if (item.MultiplyBySpawnpointsNumber)
            {
                num *= (float)list.Count;
            }
            int num2 = 0;
            while ((float)num2 < num && list.Count != 0)
            {
                ItemType itemType = EnumUtils<ItemType>.Values.GetRandomValue(i => i != ItemType.None);
                if (itemType != ItemType.None)
                {
                    int index = UnityEngine.Random.Range(0, list.Count);
                    Transform transform = list[index].Occupy();
                    if (EventManager.ExecuteEvent(new ItemSpawnedEvent(itemType, transform.transform.position)))
                    {
                        __instance.CreatePickup(itemType, transform, list[index].TriggerDoorName);
                        if (!list[index].CanSpawn(itemType))
                        {
                            list.RemoveAt(index);
                        }
                    }
                }
                num2++;
            }
            ListPool<ItemSpawnpoint>.Shared.Return(list);

            return false;
        } 
    }

    // MOST IMPORTANT ONE
    [HarmonyPatch(typeof(ItemDistributor), nameof(ItemDistributor.PlaceSpawnables))]
    public class PlaceSpawnablesPatch
    {
        public static bool Prefix(ItemDistributor __instance)
        {
            while (ItemSpawnpoint.RandomInstances.Remove(null))
            {
            }
            while (ItemSpawnpoint.AutospawnInstances.Remove(null))
            {
            }
            foreach (SpawnableItem item in __instance.Settings.SpawnableItems)
            {
                __instance.PlaceItem(item);
            }
            foreach (ItemSpawnpoint itemSpawnpoint in ItemSpawnpoint.AutospawnInstances)
            {
                Transform t = itemSpawnpoint.Occupy();
                __instance.CreatePickup(itemSpawnpoint.AutospawnItem, t, itemSpawnpoint.TriggerDoorName);
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(ItemSpawnpoint), nameof(ItemSpawnpoint.CanSpawn), new Type[] { typeof(ItemType)})]
    public class CanSpawnPatch
    {
        public static bool Prefix(ItemSpawnpoint __instance, ItemType targetItem, ref bool __result)
        {
            if (__instance._uses >= __instance.MaxUses)
            {
                __result = false;
                return false;
            }
            __result = true;
            return false;
        }
    }
}
