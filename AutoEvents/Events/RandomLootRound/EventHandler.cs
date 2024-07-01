using AutoEvents.Models;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.API.Features.Pickups;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Events.RandomLootRound
{
    public class EventHandler
    {
        private readonly Config _config;

        public EventHandler(Config config)
        {
            _config = config;
        }
    }
}
