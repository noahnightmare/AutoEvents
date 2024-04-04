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

namespace AutoEvents.Events.PeanutSurvival
{
    public class Config : EventConfig
    {
        public RoleTypeId peanutRole = RoleTypeId.Scp173;
        public RoomType Room { get; set; } = RoomType.Hcz079;
        public Vector3 PeanutRelativePosition { get; set; } = new Vector3(11.55f, -2.37f, 1.02f);
        public Vector3 PlayerRelativePosition { get; set; } = new Vector3(-3.67f, -4.28f, -6.71f);

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.ClassD,
        };
    }
}
