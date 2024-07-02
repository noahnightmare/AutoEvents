using Exiled.API.Features;
using Exiled.Loader;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using AutoEvents.Models;
using AutoEvents.Controllers;
using AutoEvents.Interfaces;
using MEC;
using HarmonyLib;

namespace AutoEvents
{
    public class AutoEvents : Plugin<Config>
    {
        public static AutoEvents Instance;
        public override string Author { get; } = "noah";
        public override string Name { get; } = "AutoEvents";
        public override string Prefix => Name;
        public override Version RequiredExiledVersion { get; } = new Version(8, 8, 0);
        public override Version Version { get; } = new Version(3, 0, 0);

        public static Harmony _harmony;
        public static string HarmonyId { get; } = "autoevents.noah.dev";

        public CooldownController CooldownController;

        private EventHandlers _handlers;

        public static IEvent currentEvent;

        public static bool isEventRunning = false;

        public static bool isEventVoteRunning = false;

        public static bool shouldDisallowEventsThisRound = false;

        public override void OnEnabled()
        {
            Instance = this;

            Event.RegisterInternalEvents();

            CooldownController = new CooldownController();

            _handlers = new EventHandlers();
            _handlers.Init();

            RegisterPatch();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;

            CooldownController.Destroy();
            CooldownController = null;

            _handlers.UnInit();
            _handlers = null;

            UnregisterPatch();

            base.OnDisabled();
        }

        // NOT PATCHING HERE! HAVE TO REGISTER INDIVIDUAL PATCH ON EACH EVENT
        // SEE RandomLootRound.cs
        private void RegisterPatch()
        {
            try
            {
                _harmony = new(HarmonyId);
            }
            catch (HarmonyException ex)
            {
                Log.Error($"[RegisterPatch] Patching Failed : {ex}");
            }
        }

        private void UnregisterPatch()
        {
            _harmony.UnpatchAll();
            _harmony = null;
        }
    }
}
