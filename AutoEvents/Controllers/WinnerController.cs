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
        public static Player winner { get; set; } = null;
        public static Side winnerSide { get; set; } = Side.None;
        public static RoleTypeId winnerDesiredRole { get; set; } = RoleTypeId.None;
        public static bool canUseRoleCommand { get; set; } = false;

        private static CoroutineHandle internalRoleChoiceCoroutine;

        public static void HandleEventWinner(Player p, Side s, string broadcastMessage)
        {
            // Assign null if there is no winner player/side
            winner = p;
            winnerSide = s;

            Map.ClearBroadcasts();

            // Winner player
            if (winner != null)
            {
                Map.Broadcast(30, broadcastMessage.Replace("{name}", winner.Nickname));
                canUseRoleCommand = true;
                internalRoleChoiceCoroutine = Timing.RunCoroutine(RoleOnEnded().CancelWith(winner.GameObject), "Role On Ended");
            }
            // Winner side
            else if (winnerSide != Side.None)
            {
                Map.Broadcast(30, broadcastMessage.Replace("{side}", Helpers.GetSideName(winnerSide)));
            }
            // No winner
            else
            {
                Map.Broadcast(30, "<b>Everyone died!</b>\nNobody won the event this time.");
            }
        }

        private static IEnumerator<float> RoleOnEnded()
        {
            while (true)
            {
                winner.ShowHint($"<b>You won the event, select a role to play as on the next round\nUse command .role RoleType\nRoles:\n<color=orange>ClassD</color>\n<color=#808080>FacilityGuard</color>\n<color=yellow>Scientist</color>\n<color=red>Scp049\nScp173\nScp939\nScp096\nScp106\nScp079\nScp3114</color>\n<color=green>Role Selected</color>: <color={winnerDesiredRole.GetColor().ToHex()}>{winnerDesiredRole}</color></b>", 1.1f);
                yield return Timing.WaitForSeconds(1f);
            }
        }

        public static IEnumerator<float> RoleOnRoundStart(Player player)
        {
            while (!Round.IsStarted)
            {
                player.ShowHint($"\n\n\n\n\n\n\n\n\n<b>You won the event, select a role to play as this round\nUse command .role RoleType\nRoles:\n<color=orange>ClassD</color> | <color=#808080>FacilityGuard</color> | <color=yellow>Scientist</color>\n<color=red>Scp049 | Scp173 | Scp939 | Scp096 | Scp106 | Scp079 | Scp3114</color>\n<color=green>Role Selected</color>: <color={winnerDesiredRole.GetColor().ToHex()}>{winnerDesiredRole}</color></b>", 1.1f);
                yield return Timing.WaitForSeconds(1f);
            }
            yield break;
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
            canUseRoleCommand = false; // not needed really but it's here to reset the variable (its set to false on round start)
            Timing.KillCoroutines(new CoroutineHandle[] { internalRoleChoiceCoroutine });
        }
    }
}
