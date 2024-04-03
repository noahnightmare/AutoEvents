using AutoEvents.Commands;
using AutoEvents.Extensions;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Controllers
{
    public static class WinnerController
    {
        public static Player winner { get; set; }
        public static Side winnerSide { get; set; }
        public static RoleTypeId winnerDesiredRole { get; set; }
        public static CoroutineHandle internalRoleChoiceCoroutine;

        public static void HandleEventWinner(Player p, Side s, string broadcastMessage)
        {
            // Assign null if there is no winner player/side
            winner = p;
            winnerSide = s;

            Map.ClearBroadcasts();
            Map.Broadcast(30, broadcastMessage.Replace("{name}", winner.Nickname).Replace("{side}", Helpers.GetSideName(winnerSide)));

            internalRoleChoiceCoroutine = Timing.RunCoroutine(RoleOnEnded(winner).CancelWith(winner.GameObject), "Role On Ended");
        }

        private static IEnumerator<float> RoleOnEnded(Player player)
        {
            while (true)
            {
                yield return Timing.WaitForSeconds(1f);
                player.ShowHint($"<b>You won the event, select a role to play as on the next round\nUse command .role RoleType\nRoles:\n<color=orange>ClassD</color>\n<color=#808080>FacilityGuard</color>\n<color=yellow>Scientist</color>\n<color=red>Scp049\nScp173\nScp939\nScp096\nScp106\nScp079\nScp3114</color>\n<color=green>Role Selected</color>: <color={winnerDesiredRole.GetColor().ToHex()}>{winnerDesiredRole}</color></b>", 1.1f);
            }
        }

        public static void Reset()
        {
            winner = null;
            winnerSide = Side.None;
            winnerDesiredRole = RoleTypeId.None;
            Kill();
        }

        public static void Kill()
        {
            Timing.KillCoroutines(new CoroutineHandle[] { internalRoleChoiceCoroutine });
        }
    }
}
