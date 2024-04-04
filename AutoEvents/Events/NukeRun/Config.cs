using AutoEvents.Interfaces;
using AutoEvents.Models;
using Exiled.API.Enums;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoEvents.Events.NukeRun
{
    public class Config : EventConfig
    {
        public int AmountOf207ToGive { get; set; } = 4;

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>();
    }
}
