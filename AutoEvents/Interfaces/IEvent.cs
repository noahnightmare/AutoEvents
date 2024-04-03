using AutoEvents.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Interfaces
{
    public interface IEvent
    {
        string Name { get; }
        EventType eventType { get; }

        void StartEvent();
        void StopEvent();
    }
}
