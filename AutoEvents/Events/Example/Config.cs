using AutoEvents.Interfaces;
using AutoEvents.Models;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Events.Example
{
    public class Config : EventConfig, IHidden
    {
        // Use defaults if override isn't necessary
        public override string EndMessage { get; set; } = "{name} won! {side} won!";
        public override RoleTypeId Role { get; set; } = RoleTypeId.ClassD;
        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>() 
        { 
            RoleTypeId.ClassD 
        };
    }
}
