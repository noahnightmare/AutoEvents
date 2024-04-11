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

namespace AutoEvents.Events.ProtectThePresident
{
    public class Config : EventConfig
    {
        public override string EndMessage { get; set; } = "<b>Congratulations to the <color=purple>{side}</color> team!\nThey won the event!</b>";
        public RoleTypeId MainRole { get; set; } = RoleTypeId.ChaosRifleman;
        public RoleTypeId PresidentRole { get; set; } = RoleTypeId.Scientist;
        public RoleTypeId PresidentGuardRole { get; set; } = RoleTypeId.NtfCaptain;
        public RoomType MainRoom { get; set; } = RoomType.EzIntercom;
        public RoomType PresidentRoom { get; set; } = RoomType.Lcz173;
        public Vector3 MainRelativePosition { get; set; } = new Vector3(-0.12f, 0.96f, 0.10f);
        public Vector3 PresidentRelativePosition { get; set; } = new Vector3(18.38f, 12.43f, 7.89f);

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>()
        {
            RoleTypeId.Scientist,
        };
    }
}
