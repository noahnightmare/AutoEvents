using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Extensions
{
    public static class PlayerExtensions
    {
        public static bool HasLocalCooldown(this Player p) => AutoEvents.Instance.CooldownController._localCooldowns.ContainsKey(p.UserId);
        public static void CreateLocalCooldown(this Player p) => AutoEvents.Instance.CooldownController.CreateLocalCooldown(p.UserId);
        public static Models.LocalCooldown LocalCooldown(this Player p) => AutoEvents.Instance.CooldownController._localCooldowns[p.UserId];
    }
}
