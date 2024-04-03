using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Models
{
    public class Cooldown
    {
        public int RemainingRoundsForAutoEvent { get; set; } = AutoEvents.Instance.Config.AutoEventAfterRounds;
        public int GlobalCooldown { get; set; } = AutoEvents.Instance.Config.GlobalCooldown;
    }
}
