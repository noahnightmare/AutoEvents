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

namespace AutoEvents.Events.HideAndSeekEntrance
{
    public class Config : EventConfig
    {
        public Vector3 Scale { get; set; } = new Vector3(0.3f, 0.3f, 0.3f);
        public RoleTypeId SeekerRole { get; set; } = RoleTypeId.Scp049;
        public RoleTypeId deadPlayerRole { get; set; } = RoleTypeId.Scp0492;
        public RoomType SeekerRoom { get; set; } = RoomType.EzGateB;
        public Vector3 SeekerRelativePosition { get; set; } = new Vector3(0.03f, 0.96f, 4.85f);
        public RoomType PlayerRoom { get; set; } = RoomType.EzCheckpointHallway;
        public Vector3 PlayerRelativePosition { get; set; } = new Vector3(-1.90f, 0.96f, 0.01f);
        public int TimeToLetHidersHide { get; set; } = 30;

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.ClassD,
        };
    }
}
