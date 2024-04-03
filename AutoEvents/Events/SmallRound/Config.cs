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

namespace AutoEvents.Events.SmallRound
{
    public class Config : EventConfig
    {
        public override string EndMessage { get; set; } = "";
        public override RoleTypeId Role { get; set; } = RoleTypeId.None;
        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>();
        public Vector3 Scale { get; set; } = new Vector3(0.5f, 0.5f, 0.5f);
    }
}
