using AutoEvents.Models;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Server;
using Exiled.Events.EventArgs.Warhead;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Events.ZombieEscape
{
    public class EventHandler
    {
        private readonly Config _config;

        public EventHandler(Config config)
        {
            _config = config;
        }

        public void OnRespawningTeam(RespawningTeamEventArgs ev) => ev.IsAllowed = false;
        public void OnWarheadStarting(StartingEventArgs ev) => ev.IsAllowed = false;
        public void OnPickingUpItem(PickingUpItemEventArgs ev)
        {
            if (_config.rolesThatCantPickup.Contains(ev.Player.Role.Type))
            {
                ev.IsAllowed = false;
            }
        }

        public void OnAnnouncingScpTermination(AnnouncingScpTerminationEventArgs ev) => ev.IsAllowed = false;

        public void OnPlayerShooting(ShootingEventArgs ev)
        {
            if (ev.Item.Type != ItemType.ParticleDisruptor)
            {
                ev.Player.AddAmmo(ev.Firearm.AmmoType, 1);
            }
        }

        public void OnPlayerSpawningRagdoll(SpawningRagdollEventArgs ev)
        {
            if (ev.Player == null) return;

            if (ev.Role == _config.ZombieRole)
            {
                ev.IsAllowed = false;
            }
        }
    }
}
