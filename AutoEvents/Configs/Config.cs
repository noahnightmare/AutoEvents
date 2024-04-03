using AutoEvents.API.Attributes;
using AutoEvents.Extensions;
using AutoEvents.Interfaces;
using AutoEvents.Models;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using Exiled.Events.Commands.Reload;
using Exiled.Loader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using YamlDotNet.Serialization;

namespace AutoEvents
{
    public class Config : IConfig
    {
        public bool IsEnabled { get; set; } = true;
        public bool Debug { get; set; } = false;

        [Description("Minimum amount of players needed on the server for someone to request an event.")]
        public int MinimumPlayersToRequest { get; set; } = 12;

        [Description("Global cooldown for events - this represents how many rounds need to pass before another event is requested.")]
        public int GlobalCooldown { get; set; } = 2;

        [Description("Local cooldown for events - this represents how many rounds need to pass before another event can be requested by the same person.")]
        public int LocalCooldown { get; set; } = 5;

        [Description("Hosts an automatic event after the specified amount of rounds.")]
        public int AutoEventAfterRounds { get; set; } = 5;

        [Description("Directory for all configs related to AutoEvents")]
        public string ConfigsFolder { get; set; } = Path.Combine(Paths.Configs, "AutoEvents"); // not actually used for anything on this version
        
    }
}