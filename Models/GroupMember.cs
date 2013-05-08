using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyGroups.Models
{
    public class GroupMember
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public int ParticipantId { get; set; }
        public int GroupParticipantId { get; set; }
        public string DisplayName { get; set; }
        public string RoleTitle { get; set; }
        public string EmailAddress { get; set; }
        public string MobilePhone { get; set; }
        public string HomePhone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string ImagePath { get; set; }
        public string DomainGuid { get; set; }
        public string FileName { get; set; }
        public bool IsMeetingAttendee { get; set; }

    }
}