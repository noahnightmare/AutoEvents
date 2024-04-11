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

namespace AutoEvents.Events.TeamDeathmatch
{
    public class Config : EventConfig
    {
        public override string EndMessage { get; set; } = "<b>Congratulations to the <color=purple>{side}</color> team!\nThey won the event!</b>";
        public RoleTypeId FirstRole { get; set; } = RoleTypeId.ChaosRifleman;
        public RoleTypeId SecondRole { get; set; } = RoleTypeId.NtfSergeant;
        public RoomType FirstRoom { get; set; } = RoomType.Surface;
        public RoomType SecondRoom { get; set; } = RoomType.Surface;
        public Vector3 FirstRelativePosition { get; set; } = new Vector3(-10.19f, 1000.96f, 2.84f);
        public Vector3 SecondRelativePosition { get; set; } = new Vector3(136.37f, 995.69f, -21.97f);

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>();
    }
}
