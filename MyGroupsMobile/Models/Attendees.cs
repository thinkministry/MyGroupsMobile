using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGroups.Models
{
    public class Attendees
    {
        public int[] ParticipantIds { get; set; }
        public int[] GroupParticipantIds { get; set; }
    }
}