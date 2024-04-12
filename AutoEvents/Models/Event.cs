using AutoEvents.API.Attributes;
using AutoEvents.Commands;
using AutoEvents.Controllers;
using AutoEvents.Enums;
using AutoEvents.Extensions;
using AutoEvents.Interfaces;
using Discord;
using Exiled.API.Enums;
using Exiled.API.Extensions;
using Exiled.API.Features;
using MEC;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoEvents.Models
{
    // Event Template inspired by AutoEvent
    public abstract class Event : IEvent
    {
        // A list of all registered events
        public static List<Event> Events { get; set; } = new List<Event>();

        // Registers all events
        internal static void RegisterInternalEvents()
        {
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            Type[] types = callingAssembly.GetTypes();

            foreach (Type type in types)
            {
                try
                {
                    if (type.IsAbstract ||
                        type.IsEnum ||
                        type.IsInterface || type.GetInterfaces().All(x => x != typeof(IEvent)))
                        continue;

                    object evBase = Activator.CreateInstance(type);
                    if (evBase is null || evBase is not Event ev ||
                    type.GetCustomAttributes(typeof(DisabledFeaturesAttribute), false).Any(x => x is not null))
                        continue;

                    if (ev is IHidden)
                    {
                        Log.Warn($"Skipping registration of the \"{ev.Name}\" event... The event implements IHidden and therefore will be ignored.");
                        continue;
                    }

                    ev.Id = Events.Count;

                    try
                    {
                        ev.InstantiateEvent();
                    }
                    catch (Exception e)
                    {
                        Log.Error($"[EventLoader] {ev.Name} encountered an error while registering.");
                        Log.Error($"[EventLoader] {e}");
                    }

                    Events.Add(ev);

                    Log.Info($"[EventLoader] {ev.Name} has been registered.");
                }
                catch (MissingMethodException) { }
                catch (Exception ex)
                {
                    Log.Error($"[EventLoader] Error when trying to register an event.");
                    Log.Error($"{ex}");

                }
            }
        }

        /// <summary>
        /// Gets the event by name. Checks ID first, and if you can't parse it as an ID, check the command name.
        /// </summary>
        public static Event GetEvent(string type)
        {
            Event ev = null;

            if (int.TryParse(type, out int id))
                return GetEvent(id);

            if (!TryGetEventByCName(type, out ev))
                return Events.FirstOrDefault(ev => ev.Name.ToLower() == type.ToLower());

            return ev;
        }

        public static Event GetEvent(int id) => Events.FirstOrDefault(x => x.Id == id);

        private static bool TryGetEventByCName(string type, out Event ev)
        {
            return (ev = Events.FirstOrDefault(x => x.CommandName == type)) != null;
        }

        // Event name & details
        public abstract string Name { get; set; }
        // Id of the event is set by the plugin
        public int Id { get; internal set; } 
        public abstract EventType eventType { get; set; }

        // Command name/prefix of the plugin
        public abstract string CommandName { get; set; }

        // Delay for restarting the round
        protected virtual float DelayForRestartingTheRound { get; set; } = 20f;

        // Force specific friendly fire settings
        protected virtual bool EnableFriendlyFire { get; set; } = false;

        // -----------------------------------------------------------------------------

        protected virtual float coroutineDelay { get; set; } = 1f;

        // Used to safely kill the while loop, without have to forcibly kill the coroutine
        protected virtual bool KillLoops { get; set; } = false;

        // The coroutine handle of the main event thread calling ProcessLogic
        protected virtual CoroutineHandle EventCoroutine { get; set; }

        // Method called every second
        protected virtual void ProcessEventLogic() { }

        public virtual DateTime StartTime { get; protected set; }

        public virtual TimeSpan EventTime { get; protected set; }


        // -----------------------------------------------------------------------------

        // Run the event safely
        public void StartEvent()
        {
            Log.Info($"Starting event: {Name}");
            OnInternalStart();
        }

        public void StopEvent()
        {
            Log.Info($"Stopping event: {Name}");
            OnInternalStop();
        }

        // base constructor for an event
        public Event() { }

        public virtual void InstantiateEvent() { }

        protected abstract void OnStart();

        protected virtual void RegisterEvents() { }

        protected abstract bool IsEventDone();

        protected virtual void OnStop() { }

        protected abstract void OnEnd();

        protected virtual void UnregisterEvents() { }

        protected virtual void OnCleanup() { }

        // Main game loop
        // generally avoid overrides to this method, main thing is that it calls the event's main method ProcessEventLogic
        protected virtual IEnumerator<float> RunEventCoroutine()
        {
            while (!IsEventDone())
            {
                if (KillLoops)
                {
                    yield break;
                }
                try
                {
                    ProcessEventLogic();
                }
                catch (Exception e)
                {
                    Log.Error($"Error at Event.RunEventCoroutine().");
                    Log.Error($"{e}");
                }

                EventTime += TimeSpan.FromSeconds(coroutineDelay);
                yield return Timing.WaitForSeconds(coroutineDelay);
            }
            yield break;
        }

        // ---------------------------------------------------------------------------------------------------

        // internal action to stop the event
        private void OnInternalStop()
        {
            KillLoops = true;
            Server.FriendlyFire = false;

            Timing.CallDelayed(coroutineDelay + .1f, () =>
            {
                if (EventCoroutine.IsRunning)
                {
                    Timing.KillCoroutines(new CoroutineHandle[] { EventCoroutine });
                }
                OnInternalCleanup();
            });

            try
            {
                OnStop();
            }
            catch (Exception e)
            {

                Log.Error($"Caught an exception at Event.OnStop()");
                Log.Error($"{e}");
            }
            EventStopped?.Invoke(Name);
        }

        // internal action to start the event
        private void OnInternalStart()
        {
            KillLoops = false;
            _cleanupRan = false;
            EventTime = new TimeSpan();
            StartTime = DateTime.UtcNow;

            Server.FriendlyFire = EnableFriendlyFire;

            try
            {
                RegisterEvents();
            }
            catch (Exception e)
            {
                Log.Error($"Error at Event.RegisterEvents()");
                Log.Error($"{e}");
            }

            Map.ClearBroadcasts();

            try
            {
                OnStart();
            }
            catch (Exception e)
            {

                Log.Error($"Error at Event.OnStart()");
                Log.Error($"{e}");
            }

            EventStarted?.Invoke(Name);

            Timing.RunCoroutine(RunTimingCoroutine(), "TimingCoroutine");
        }

        // Main loop for everything
        private IEnumerator<float> RunTimingCoroutine()
        {
            EventCoroutine = Timing.RunCoroutine(RunEventCoroutine(), "Event Coroutine");
            yield return Timing.WaitUntilDone(EventCoroutine);
            if (KillLoops)
            {
                yield break;
            }
            
            try
            {
                OnEnd();
            }
            catch (Exception e)
            {
                Log.Error($"Error at Event.OnEnd()");
                Log.Error($"{e}");
            }

            var handle = Timing.CallDelayed(DelayForRestartingTheRound, () =>
            {
                if (!_cleanupRan)
                {
                    OnInternalCleanup();
                }
            });
            yield return Timing.WaitUntilDone(handle);
        }

        // Used to only run cleanup once
        private bool _cleanupRan = false;

        private void OnInternalCleanup()
        {
            _cleanupRan = true;
            Server.FriendlyFire = false;
            try
            {
                UnregisterEvents();
            }
            catch (Exception e)
            {
                Log.Error($"Error at Event.OnUnregisterEvents()");
                Log.Error($"{e}");
            }

            try
            {
                if (eventType != EventType.NormalRound)
                {
                    Helpers.CleanUpAll();
                }
            }
            catch (Exception e)
            {
                Log.Error($"Error at Event.Helpers.CleanUpAll()");
                Log.Error($"{e}");
            }

            try
            {
                OnCleanup();
            }
            catch (Exception e)
            {
                Log.Error($"Error at Event.OnCleanup()");
                Log.Error($"{e}");
            }

            // StartTime = null;
            // EventTime = null;
            try
            {
                CleanupFinished?.Invoke(Name);
            }
            catch (Exception e)
            {
                Log.Error($"Error at Event.CleanupFinished.Invoke()");
                Log.Error($"{e}");
            }

            if (eventType == EventType.Event)
            {
                Round.Restart(false);
            }
        }

        // delegates handling the current point in time that the event is in

        public delegate void EventStoppedHandler(string eventName);
        public delegate void CleanupFinishedHandler(string eventName);
        public delegate void EventStartedHandler(string eventName);

        /// Called when the event start is triggered.
        public virtual event EventStartedHandler EventStarted;

        /// Called when the event cleanup is finished. The event is completely finished and disposed of once this is called. 
        public virtual event CleanupFinishedHandler CleanupFinished;

        /// Called when the event is stopped. When the event is stopped, OnFinished() won't be called, but OnCleanup() will be called.
        public virtual event EventStoppedHandler EventStopped;
    }
}