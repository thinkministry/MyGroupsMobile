using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGroups.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Ministry { get; set; }
        public int MinistryId { get; set; }
        public int CongregationId { get; set; }

    }
}