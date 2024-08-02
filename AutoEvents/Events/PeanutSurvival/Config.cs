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
        public Vector3 PeanutRelativePosition { get; set; } = new Vector3(11.93f, -2.37f, -0.03f);
        public Vector3 PlayerRelativePosition { get; set; } = new Vector3(6.61f, -2.37f, -0.06f);

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.ClassD,
        };
    }
}
