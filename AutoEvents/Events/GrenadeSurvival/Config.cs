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

namespace AutoEvents.Events.GrenadeSurvival
{
    public class Config : EventConfig
    {
        public float Health { get; set; } = 200f;
        public RoomType Room { get; set; } = RoomType.Lcz173;
        public Vector3 RelativePosition { get; set; } = new Vector3(16.41f, 15.05f, 7.84f);
        public float StartTimer { get; set; } = 10f;

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.ClassD,
        };
    }
}
