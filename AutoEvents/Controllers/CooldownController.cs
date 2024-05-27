using AutoEvents.Models;
using Exiled.API.Features;
using Exiled.Loader;
using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Handler = Exiled.Events.Handlers;

namespace AutoEvents.Controllers
{
    public class CooldownController
    {
        private static string Directory = Path.Combine(Paths.Plugins, "AutoEvents");
        private static string CooldownPath = Path.Combine(Directory, Server.Port.ToString());
        private static string GlobalCooldown = Path.Combine(CooldownPath, "GlobalCooldown.yml");
        public Dictionary<string, LocalCooldown> _localCooldowns = new Dictionary<string, LocalCooldown>();
        public Cooldown _cooldown;
        private List<CoroutineHandle> _cooldownControllerCoroutines = new List<CoroutineHandle>();

        // Handles queuing an event for next round
        private List<QueuedEvent> _queuedEvents = new List<QueuedEvent>();

        public CooldownController()
        {
            Handler.Server.WaitingForPlayers += OnWaitingForPlayers;
            Handler.Server.RoundStarted += OnRoundStarted;

            // Make the directory if it doesn't exist
            if (!System.IO.Directory.Exists(CooldownPath))
            {
                System.IO.Directory.CreateDirectory(CooldownPath);

                _cooldown = new Cooldown
                {
                    GlobalCooldown = 0,
                    RemainingRoundsForAutoEvent = AutoEvents.Instance.Config.AutoEventAfterRounds
                };

                File.WriteAllText(GlobalCooldown, Loader.Serializer.Serialize(_cooldown));
            }

            // Reset local and global cooldowns
            _localCooldowns.Clear();
            _cooldown = Loader.Deserializer.Deserialize<Cooldown>(File.ReadAllText(GlobalCooldown));

            // Get people's local cooldowns from the file and store it within the LocalCooldowns dict
            foreach (string file in System.IO.Directory.GetFiles(Directory))
            {
                string name = Path.GetFileName(file);
                if (!name.Contains("GlobalCooldown"))
                {
                    string userId = "";
                    if (name.Contains("."))
                    {
                        userId = name.Split('.')[0];
                    }
                    else
                    {
                        userId = name;
                    }

                    _localCooldowns.Add(userId, Loader.Deserializer.Deserialize<LocalCooldown>(File.ReadAllText(file)));
                }
            }
        }

        // uninitialise and save final values to directory
        public void Destroy()
        {
            File.WriteAllText(GlobalCooldown, Loader.Serializer.Serialize(_cooldown));

            foreach (KeyValuePair<string, LocalCooldown> playerCooldown in _localCooldowns)
            {
                File.WriteAllText(Path.Combine(Directory, playerCooldown.Key + ".yml"), Loader.Serializer.Serialize(playerCooldown.Value));
            }

            Handler.Server.WaitingForPlayers -= OnWaitingForPlayers;
            Handler.Server.RoundStarted -= OnRoundStarted;
        }

        public void QueueEvent(Event ev, Player requestedPlayer)
        {
            _queuedEvents.Add(new QueuedEvent()
            {
                Event = ev,
                requestedPlayer = requestedPlayer
            });
        }

        // Kills coroutine when round starts to avoid it unnecessarily running
        public void OnRoundStarted()
        {
            foreach (CoroutineHandle coro in _cooldownControllerCoroutines)
            {
                Timing.KillCoroutines(coro);
            }
        }

        public void OnWaitingForPlayers()
        {
            // decrement the global cooldown if it isnt already 0
            if (_cooldown.GlobalCooldown != 0)
            {
                _cooldown.GlobalCooldown -= 1;
            }

            // check localcooldowns and decrement them if necessary
            foreach (LocalCooldown playerCooldown in _localCooldowns.Values)
            {
                if (playerCooldown.RemainingCooldownRounds > 0)
                {
                    playerCooldown.RemainingCooldownRounds -= 1;
                }
            }

            // Queues event if it was asked for last round
            if (!_queuedEvents.IsEmpty())
            {
                new EventController(_queuedEvents[0].Event, _queuedEvents[0].requestedPlayer);
                _queuedEvents?.RemoveAt(0);
            }

            // provoke coroutine to check values if an event can be started, if remaining rounds is 0
            if (_cooldown.RemainingRoundsForAutoEvent == 0)
            {
                _cooldownControllerCoroutines.Add(Timing.RunCoroutine(AutoEvent(), "Check Auto Event"));
                if (AutoEvents.Instance.CooldownController._cooldown.GlobalCooldown <= 2) { AutoEvents.Instance.CooldownController._cooldown.GlobalCooldown = 2; } // fixes issues with event being requestable after an auto event
            }
            else _cooldown.RemainingRoundsForAutoEvent -= 1; // decrement remaining rounds if not 0

            Save();
        }

        private IEnumerator<float> AutoEvent()
        {
            while (!Round.IsStarted && !AutoEvents.isEventRunning && !AutoEvents.isEventVoteRunning && _cooldown.RemainingRoundsForAutoEvent == 0)
            {
                if (/* Player.List.Count() >= AutoEvents.Instance.Config.MinimumPlayersToRequest */ true)
                {
                    StartEventVoting();
                    yield break;
                }
                yield return Timing.WaitForSeconds(1f);
            }
            yield break;
        }

        // starts the voting sequence for events
        public void StartEventVoting()
        {
            if (AutoEvents.isEventRunning || AutoEvents.isEventVoteRunning) return;

            Log.Info($"Event voting started!");
            new EventVoteController();
        }

        // create a new local cooldown for someone who doesn't have one
        public void CreateLocalCooldown(string userid)
        {
            LocalCooldown lc = new LocalCooldown
            {
                RemainingCooldownRounds = AutoEvents.Instance.Config.LocalCooldown
            };

            _localCooldowns.Add(userid, lc);
        }

        // Saves updated cooldown in a file
        public void Save()
        {
            File.WriteAllText(GlobalCooldown, Loader.Serializer.Serialize(_cooldown));

            foreach (KeyValuePair<string, LocalCooldown> plycooldown in _localCooldowns)
            {
                File.WriteAllText(Path.Combine(Directory, plycooldown.Key + ".yml"), Loader.Serializer.Serialize(plycooldown.Value));
            }
        }
    }
}
