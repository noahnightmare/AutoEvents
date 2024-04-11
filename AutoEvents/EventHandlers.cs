using AutoEvents.Commands;
using AutoEvents.Controllers;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;

using Handler = Exiled.Events.Handlers;

namespace AutoEvents
{
    public class EventHandlers
    {
        Random rng = new Random();
        private RoleTypeId winnerPreviousRole = RoleTypeId.ClassD;

        public void Init()
        {
            Handler.Server.RoundStarted += OnRoundStart;
            Handler.Server.WaitingForPlayers += OnWaitingForPlayers;
        }

        public void UnInit()
        {
            Handler.Server.RoundStarted -= OnRoundStart;
            Handler.Server.WaitingForPlayers -= OnWaitingForPlayers;
        }

        private IEnumerator<float> SwitchRoles(Player player, RoleTypeId role, bool isWinner)
        {
            yield return Timing.WaitForSeconds(2f);
            Log.Info("Switch roles done! IsWinner: " + isWinner);
            if (player != null)
            {
                Log.Info("Swapping role into: " + role.ToString());

                if (!isWinner)
                {
                    player.Broadcast(5, "<b>Your role was chosen by the <color=green>winner</color> of the previous event. Sorry!</b>");
                }

                player.Role.Set(role, SpawnReason.ForceClass, RoleSpawnFlags.All);
            }
            yield break;
        }

        public void OnWaitingForPlayers()
        {
            if (AutoEvents.shouldDisallowEventsThisRound)
            {
                AutoEvents.shouldDisallowEventsThisRound = false;
            }
        }

        public void OnRoundStart()
        {
            if (WinnerController.winnerDesiredRole == RoleTypeId.None || WinnerController.winner == null)
            {
                return;
            }

            // Resets winners 15 seconds into the game
            Timing.CallDelayed(15f, WinnerController.Reset);

            List<Player> PlayersOfWinnerRole = new List<Player>();
            List<Player> PlayersAsSCP = new List<Player>();

            // Handle role swapping for the winner
            Timing.CallDelayed(0.25f, () =>
            {
                bool playerIsAlreadyWinnerRole = false;

                foreach (Player player in Player.List)
                {
                    if (player.UserId == WinnerController.winner.UserId)
                    {
                        Log.Warn($"{player.Nickname} - winner found!");
                        winnerPreviousRole = player.Role;
                        if (player.Role != WinnerController.winnerDesiredRole)
                        {
                            Log.Info("The winner didn't spawn as their role. Switching roles...");
                            Timing.RunCoroutine(SwitchRoles(player, WinnerController.winnerDesiredRole, true));
                        }
                        else playerIsAlreadyWinnerRole = true;
                    }

                    if (player.Role == WinnerController.winnerDesiredRole && player.UserId != WinnerController.winner.UserId)
                    {
                        PlayersOfWinnerRole.Add(player);
                    }

                    if (RoleExtensions.GetTeam(player.Role) == Team.SCPs && player.UserId != WinnerController.winner.UserId)
                    {
                        PlayersAsSCP.Add(player);
                    }
                }

                if (!playerIsAlreadyWinnerRole)
                {
                    if (!PlayersOfWinnerRole.IsEmpty())
                    {
                        int index = rng.Next(0, PlayersOfWinnerRole.Count);
                        Log.Info("PlayersOfWinnerRole is not empty! Swapping " + PlayersOfWinnerRole[index].ToString() + " to: " + winnerPreviousRole.ToString());
                        Timing.RunCoroutine(SwitchRoles(PlayersOfWinnerRole[index], winnerPreviousRole, false));
                    }
                    else
                    {
                        if (RoleExtensions.GetTeam(WinnerController.winnerDesiredRole) == Team.SCPs)
                        {
                            if (PlayersAsSCP.Count == 0)
                            {
                                Log.Warn("PlayersAsSCP list has 0 players!");
                            }
                            else
                            {
                                int index = rng.Next(0, PlayersAsSCP.Count);
                                Log.Info("PlayersOfWinnerRole is empty, and there was a non winner SCP! Swapping " + PlayersAsSCP[index].ToString() + " to: " + winnerPreviousRole.ToString());
                                Timing.RunCoroutine(SwitchRoles(PlayersAsSCP[index], winnerPreviousRole, false));
                            }
                        }
                    }
                }

                PlayersOfWinnerRole?.Clear();
                PlayersAsSCP?.Clear();
                winnerPreviousRole = RoleTypeId.None;
                PlayersOfWinnerRole = null;
                PlayersAsSCP = null;
            });
        }
    }
}
