using AutoEvents.Commands;
using AutoEvents.Models;
using Exiled.API.Features;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Core.Events;

namespace AutoEvents.Controllers
{
    public class EventVoteController
    {
        private List<CoroutineHandle> _coroutines = new List<CoroutineHandle>();
        private static System.Random Rand = new System.Random();

        private List<Event> _possibleEvents;
        private static List<VoteEvent> _votingEvents;
        private int amountOfVotingEvents = 3;
        private static int _cancelVotes;

        private string _colourHex = "#85cf89";

        private bool _killLoops;

        public EventVoteController()
        {
            AutoEvents.isEventRunning = true;
            AutoEvents.isEventVoteRunning = true;

            _killLoops = false;

            _possibleEvents = new List<Event>(Event.Events);
            _votingEvents = new List<VoteEvent>();
            _cancelVotes = 0;

            // initialise 3 random events
            for (int i = 0; i < amountOfVotingEvents; i++)
            {
                Event eventPicked = Event.Events[Rand.Next(Event.Events.Count)];
                _votingEvents.Add(new VoteEvent { Event = eventPicked, Votes = 0 });
                _possibleEvents.Remove(eventPicked);
            }

            Map.ClearBroadcasts();
            Round.IsLobbyLocked = true;
            _coroutines.Add(Timing.RunCoroutine(ShowEventName(), "Show Event Name"));
            _coroutines.Add(Timing.RunCoroutine(WaitToCheckVotes(), "Wait Check Votes"));
        }

        private void Destroy()
        {
            AutoEvents.isEventVoteRunning = false;
            AutoEvents.isEventRunning = false;

            _killLoops = true;

            foreach (CoroutineHandle coro in _coroutines)
            {
                Timing.KillCoroutines(coro);
            }

            // clears all votes when vote is over
            VoteCommand.playerVoted?.Clear();

            _possibleEvents.Clear();
            _votingEvents?.Clear();
        }

        private IEnumerator<float> ShowEventName()
        {
            while (true)
            {
                if (_killLoops)
                {
                    yield break;
                }

                yield return Timing.WaitForSeconds(1f);
                Map.Broadcast((ushort)1.5, $"<b><color=purple>Starting an event this round...</color></b>\n<b><color=green>Event</color>: Vote below!\n<b><color=purple><size=30>Requested by</color>: <color=red>Server</color></size><size=20>", global::Broadcast.BroadcastFlags.Normal, false);

                Map.ShowHint(GenerateHintMessage());
            }
        }

        private string GenerateHintMessage()
        {
            StringBuilder hintBuilder = new StringBuilder();
            hintBuilder.AppendLine("\n\n\n\n\n\n\n\n<b>Vote for an Event by typing .vote <number> in your console!</b>");
            hintBuilder.AppendLine("Press ` to open your console. For example, .vote 1");
            hintBuilder.AppendLine("<b>The event with the highest amount of votes will be chosen.</b>\n");

            string colour;
            VoteEvent highestVotedEvent = CalculateVotedEvent();

            // append the event with it's amount of votes and name
            for (int i = 0; i < _votingEvents.Count; i++)
            {
                VoteEvent voteEvent = _votingEvents[i];

                colour = voteEvent == highestVotedEvent ? _colourHex : "white"; // Different color for the highest voted event

                hintBuilder.AppendLine($"<align=left><color=purple>                                     [{i + 1}]</color> {voteEvent.Event.Name} | <color={colour}><b>{voteEvent.Votes} votes</b></color></align>");
            }

            // This means cancel has the most votes
            if (highestVotedEvent == null)
            {
                colour = _colourHex;
            }
            else colour = "white";

            // Append cancel option
            hintBuilder.AppendLine($"<align=left><color=purple>                                     [{_votingEvents.Count + 1}]</color> Cancel the Event Round | <color={colour}><b>{_cancelVotes} votes</b></color></align>");

            return hintBuilder.ToString();
        }

        private IEnumerator<float> WaitToCheckVotes()
        {
            yield return Timing.WaitForSeconds(20f);

            Round.IsLobbyLocked = false;

            yield return Timing.WaitForSeconds(10f);

            Event outcome = CalculateVotedEvent()?.Event;

            if (outcome != null)
            {
                // initialise an event
                new EventController(outcome, Server.Host);
            }
            else
            {
                Map.Broadcast(10, "<b>Event was cancelled!</b>\nA normal round will play out.");
            }

            Destroy();
            yield break;
        }

        // returns event for event initialisation
        private VoteEvent CalculateVotedEvent()
        {
            // find the event with the highest votes
            // Orders them highest to lowest by votes, checks whether the votes are greater than or equal to cancel votes, and if so return the event, otherwise null
            return _votingEvents.OrderByDescending(v => v.Votes).FirstOrDefault().Votes >= _cancelVotes ? _votingEvents.OrderByDescending(v => v.Votes).FirstOrDefault() : null;
        }

        public static void SetVoteEventVotes(int index, int amount)
        {
            _votingEvents[index].Votes += amount;
        }

        public static void SetCancelVotes(int amount)
        {
            _cancelVotes += amount;
        }
    }
}
