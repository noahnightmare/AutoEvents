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

namespace AutoEvents.Events.DogHideAndSeek
{
    public class Config : EventConfig
    {
        public RoleTypeId SeekerRole = RoleTypeId.Scp939;
        public RoomType Room { get; set; } = RoomType.Lcz173;
        public Vector3 RelativePosition { get; set; } = new Vector3(19.64f, 12.43f, 7.98f);
        public int TimeToLetHidersHide { get; set; } = 30;

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.ClassD,
        };
    }
}
