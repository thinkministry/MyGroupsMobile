using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGroups.Models
{
    public class Meeting
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string EventTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

    }
}