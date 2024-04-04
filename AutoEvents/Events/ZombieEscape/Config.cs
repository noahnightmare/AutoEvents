using AutoEvents.Interfaces;
using AutoEvents.Models;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoEvents.Events.ZombieEscape
{
    public class Config : EventConfig
    {
        public string AlternativeEndMessage { get; set; } = "<b>Congratulations to the <color=purple>{side}</color> team!\nAll humans were killed and Zombies won!</b>";
        // Zombie configs
        public RoleTypeId ZombieRole { get; set; } = RoleTypeId.Scp0492;
        public float ZombieGodmodeTime { get; set; } = 30f;

        public RoomType FirstZombieRoom { get; set; } = RoomType.Lcz173;
        public Vector3 FirstZombieRelativePosition { get; set; } = new Vector3(19.64f, 12.43f, 7.98f);
        public float FirstZombieDamage { get; set; } = 20f;

        public RoomType SecondZombieRoom { get; set; } = RoomType.Hcz079;
        public Vector3 SecondZombieRelativePosition { get; set; } = new Vector3(11.88f, -2.37f, -9.95f);
        public float SecondZombieHealth { get; set; } = 750f;
        public float SecondZombieSpeed { get; set; } = 5f;
        public float SecondZombieDamage { get; set; } = 30f;

        public RoomType ThirdZombieRoom { get; set; } = RoomType.EzVent;
        public Vector3 ThirdZombieRelativePosition { get; set; } = new Vector3(-0.03f, -0.96f, -4.23f);
        public float ThirdZombieHealth { get; set; } = 1000f;
        public float ThirdZombieSpeed { get; set; } = 10f;
        public float ThirdZombieDamage { get; set; } = 40f;

        public RoomType FourthZombieRoom { get; set; } = RoomType.Surface;
        public Vector3 FourthZombieRelativePosition { get; set; } = new Vector3(0.94f, 991.65f, -42.82f);

        public float FourthZombieHealth { get; set; } = 1500f;
        public float FourthZombieSpeed { get; set; } = 20f;
        public float FourthZombieDamage { get; set; } = 50f;

        // Human configs
        public List<ItemType> FirstInventory { get; set; } = new List<ItemType>()
        {
            ItemType.GunCOM18,
            ItemType.Painkillers,
        };

        public List<ItemType> SecondInventory { get; set; } = new List<ItemType>()
        {
            ItemType.GunCrossvec,
            ItemType.Medkit,
        };

        public List<ItemType> ThirdInventory { get; set; } = new List<ItemType>()
        {
            ItemType.GunE11SR,
            ItemType.SCP500,
            ItemType.Medkit,
        };

        public List<ItemType> FourthInventory { get; set; } = new List<ItemType>()
        {
            ItemType.GunFRMG0,
            ItemType.ParticleDisruptor,
            ItemType.SCP500,
            ItemType.Adrenaline,
        };

        public override List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>() 
        { 
            RoleTypeId.ClassD, 
        };
    }
}
