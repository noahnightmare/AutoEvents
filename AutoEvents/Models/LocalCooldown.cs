using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Models
{
    public class LocalCooldown
    {
        public int RemainingCooldownRounds { get; set; } = AutoEvents.Instance.Config.LocalCooldown;
    }
}
