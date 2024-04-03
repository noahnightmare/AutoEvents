using AutoEvents.Models;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Events.SmallRound
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
