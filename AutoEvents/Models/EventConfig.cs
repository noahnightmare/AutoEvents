using AutoEvents.Interfaces;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Models
{
    public class EventConfig : IEventConfig
    {
        // Broadcast displayed at the end of the round determining the winner
        // Use {name} and {side} to replace with the winner's nickname/side name, but the Player/Side variable provided to WinnerController.HandleEndEvent MUST NOT be null if you use one of these
        public virtual string EndMessage { get; set; } = "<b><color=purple>{name}</color> has won the event!</b>\nThey will now pick their role for next round.";

        // Default role for the event
        public virtual RoleTypeId Role { get; set; } = RoleTypeId.ClassD;

        public virtual List<RoleTypeId> rolesThatCantPickup { get; set; } = new List<RoleTypeId>();
    }
}
