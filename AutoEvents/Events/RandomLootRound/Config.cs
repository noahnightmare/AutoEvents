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

namespace AutoEvents.Events.RandomLootRound
{
    public class Config : EventConfig
    {
        public override string EndMessage { get; set; } = "";
        public override RoleTypeId Role { get; set; } = RoleTypeId.None;
        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>();
    }
}
