using AutoEvents.Models;
using Exiled.API.Enums;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Map;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.EventArgs.Warhead;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Handler = Exiled.Events.Handlers;
using Exiled.Events.EventArgs.Server;
using Interactables.Interobjects.DoorUtils;
using AutoEvents.Commands;
using AutoEvents.Enums;
using Exiled.API.Extensions;
using AutoEvents.Extensions;

namespace AutoEvents.Controllers
{
    public class EventController
    {
        private Event _currentEvent;
        private Player _requestedPlayer;
        private List<CoroutineHandle> _coroutines;
        private bool _killLoops;

        public EventController(Event currentEvent, Player requestedPlayer) 
        {
            Log.Info("New Event Controller!!!!!!!!!!!!!!!!");
            AutoEvents.isEventRunning = true;
            AutoEvents.currentEvent = currentEvent;

            // reset the auto event cooldown
            AutoEvents.Instance.CooldownController._cooldown.RemainingRoundsForAutoEvent = AutoEvents.Instance.Config.AutoEventAfterRounds;

            _currentEvent = currentEvent;
            _requestedPlayer = requestedPlayer;

            if (_currentEvent.eventType == EventType.Event)
            {
                Round.IsLocked = true;
            }

            Log.Info($"Event happening on this round: {_currentEvent.Name}");
            Map.ClearBroadcasts();

            _killLoops = false;

            _coroutines = new List<CoroutineHandle>()
            {
                Timing.RunCoroutine(ShowEventName())
            };

            Handler.Server.RestartingRound += OnRoundRestarting;
            Handler.Server.RoundStarted += OnRoundStarted;
            Handler.Player.ChangingRole += OnChangingRole;
        } 

        public void Destroy()
        {
            Log.Info("Destroy called!!!!!!!!!!!!!!!!!!");
            AutoEvents.currentEvent = null;
            AutoEvents.isEventRunning = false;

            Round.IsLocked = false;

            _currentEvent = null;
            _requestedPlayer = null;

            _killLoops = true;
            
            foreach(CoroutineHandle coro in _coroutines)
            {
                Timing.KillCoroutines(coro);
            }

            _coroutines?.Clear();

            Handler.Server.RestartingRound -= OnRoundRestarting;
            Handler.Server.RoundStarted -= OnRoundStarted;
            Handler.Player.ChangingRole -= OnChangingRole;
        }

        private void OnRoundStarted()
        {
            Timing.CallDelayed(0.25f, () =>
            {
                // Safely start the event on round start
                _currentEvent.StartEvent();
            });
        }

        private IEnumerator<float> ShowEventName()
        {
            while (!Round.IsStarted)
            {
                if (_killLoops)
                {
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
                Map.Broadcast((ushort)1.5, $"<b><color=purple>Starting an event this round...</color></b>\n<b><color=green>Event</color>: {_currentEvent.Name}\n<b><color=purple><size=30>Requested by</color>: <color=" + GetRankColour(_requestedPlayer) + ">" + GetRankName(_requestedPlayer) + "</size><size=20>  " + _requestedPlayer.RankName + "</size></color></b>", global::Broadcast.BroadcastFlags.Normal, false);
            }
        }

        public string GetRankColour(Player player)
        {
            if (player == Server.Host)
            {
                return "red";
            }

            if (player.BadgeHidden)
            {
                return "white";
            }

            if (Enum.TryParse(player.RankColor, true, out Misc.PlayerInfoColorTypes keyRoleColour))
            {
                Misc.AllowedColors.TryGetValue(keyRoleColour, out string rankColour);
                return rankColour;
            }
            Map.ShowHint("A rank colour was not parsed properly when starting an event!\nIf this happens, screenshot this and send it to Noah\nUser: " + player.Nickname + "\nColour: " + player.RankColor + "\nPlayer verified: " + player.IsVerified, 10);
            return "";
        }

        public string GetRankName(Player player)
        {
            if (player == Server.Host)
            {
                return "Server";
            }

            if (player.BadgeHidden)
            {
                return "<i>Anonymous</i>";
            }

            return player.Nickname;
        }

        private void OnRoundRestarting()
        {
            Log.Info("Round restarting!!!!!!!!!!!!");
            Destroy();
        }

        private void OnChangingRole(ChangingRoleEventArgs ev)
        {
            if (ev.Reason == SpawnReason.LateJoin)
            {
                ev.Items.Clear();
                ev.NewRole = RoleTypeId.Spectator;
            }
        }
    }
}
