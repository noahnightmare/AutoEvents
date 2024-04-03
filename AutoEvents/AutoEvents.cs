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

namespace AutoEvents
{
    public class AutoEvents : Plugin<Config>
    {
        public static AutoEvents Instance;
        public override string Author { get; } = "noahxo";
        public override string Name { get; } = "AutoEvents";
        public override string Prefix => Name;
        public override Version RequiredExiledVersion { get; } = new Version(8, 8, 0);
        public override Version Version { get; } = new Version(3, 0, 0);

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

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Instance = null;

            CooldownController.Destroy();
            CooldownController = null;

            _handlers.UnInit();
            _handlers = null;

            base.OnDisabled();
        }
    }
}
