using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoEvents.Models
{
    // Couples votes with events to determine their vote count on voting rounds
    public class VoteEvent
    {
        public Event Event { get; set; }
        public int Votes { get; set; }
    }
}
