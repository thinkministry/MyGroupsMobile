using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGroups.Models
{
    public class MembersAndMeetings
    {
        public List<GroupMember> Members { get; set; }
        public List<Meeting> Meetings { get; set; }
    }
}