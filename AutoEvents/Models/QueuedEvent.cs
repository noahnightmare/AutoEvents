using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Models
{
    // Event schema for queued events as part of CooldownController
    public class QueuedEvent
    {
        public Event Event { get; set; }
        public Player requestedPlayer { get; set; } 
    }
}
