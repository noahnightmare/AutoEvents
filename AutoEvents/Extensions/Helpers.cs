using AutoEvents.Commands;
using Exiled.API.Enums;
using Exiled.API.Features;
using InventorySystem.Items.Pickups;
using MEC;
using PlayerRoles.Ragdolls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Exiled.Loader;

using Object = UnityEngine.Object;

namespace AutoEvents.Extensions
{
    public static class Helpers
    {
        public static void CleanUpAll()
        {
            foreach (var item in Object.FindObjectsOfType<ItemPickupBase>())
            {
                GameObject.Destroy(item.gameObject);
            }

            foreach (var ragdoll in Object.FindObjectsOfType<BasicRagdoll>())
            {
                GameObject.Destroy(ragdoll.gameObject);
            }
        }

        public static string GetSideName(Side? side)
        {
            if (!side.HasValue)
            {
                return "";
            }

            switch (side)
            {
                case Side.Scp:
                    return "SCPs";
                case Side.Mtf:
                    return "Nine Tailed Fox";
                case Side.ChaosInsurgency:
                    return "Chaos Insurgency";
                default:
                    return "Other";
            }
        }

        // helper to deserialize custom type
        public static Type DeserializeConfig<Type>(string filePath)
        {
            return Loader.Deserializer.Deserialize<Type>(File.ReadAllText(filePath));
        }
    }
}
