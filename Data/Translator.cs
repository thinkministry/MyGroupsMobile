using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;
using MyGroups.Models;
using PlatformSDK;

namespace MyGroups.Data
{
    public class Translator
    {


        public static List<Group> GetMyGroups()
        {
            List<Group> Groups = new List<Group>();
            using (DataManager dm = new DataManager(PlatformUser.Current))
            {
                DataSet dsMyGroups = dm.ExecuteProcedure("api_MGM_GetMyGroups", "ContactID=" + PlatformUser.Current.ContactId.ToString());
                for (int g = 0; g < dsMyGroups.Tables[0].Rows.Count; g++)
                {
                    Groups.Add(new Group
                    {
                        Id = int.Parse(dsMyGroups.Tables[0].Rows[g]["Group_ID"].ToString()),
                        Name = dsMyGroups.Tables[0].Rows[g]["Group_Name"].ToString(),
                        Role = dsMyGroups.Tables[0].Rows[g]["Role_Title"].ToString(),
                        Ministry = dsMyGroups.Tables[0].Rows[g]["Ministry_Name"].ToString(),
                        MinistryId = int.Parse(dsMyGroups.Tables[0].Rows[g]["Ministry_ID"].ToString()),
                        CongregationId = int.Parse(dsMyGroups.Tables[0].Rows[g]["Congregation_ID"].ToString())
                    });
                }
            }
            return Groups;
        }

        public static MembersAndMeetings GetMyGroupMembersAndMeetings(int GroupId)
        {

            MembersAndMeetings data = new MembersAndMeetings();
            List<GroupMember> Members = new List<GroupMember>();
            List<Meeting> Meetings = new List<Meeting>();

            string ImagePath = ConfigurationManager.AppSettings["ImagePath"];
            string DomainGuid = PlatformUser.Current.DomainGuid;

            using (DataManager dm = new DataManager(PlatformUser.Current))
            {
                DataSet dsMyMembersAndMeetings = dm.ExecuteProcedure("api_MGM_GetGroupMembersAndMeetings", "GroupID=" + GroupId.ToString());
                DataTable dtMembers = dsMyMembersAndMeetings.Tables[0];
                DataTable dtMeetings = dsMyMembersAndMeetings.Tables[1];
                for (int p = 0; p < dtMembers.Rows.Count; p++)
                {
                    Members.Add(new GroupMember
                    {
                        Id = int.Parse(dtMembers.Rows[p]["Group_Participant_ID"].ToString()),
                        ContactId = int.Parse(dtMembers.Rows[p]["Contact_ID"].ToString()),
                        ParticipantId = int.Parse(dtMembers.Rows[p]["Participant_ID"].ToString()),
                        GroupParticipantId = int.Parse(dtMembers.Rows[p]["Group_Participant_ID"].ToString()),
                        DisplayName = dtMembers.Rows[p]["Display_Name"].ToString(),
                        RoleTitle = dtMembers.Rows[p]["Role_Title"].ToString(),
                        EmailAddress = dtMembers.Rows[p]["Email_Address"].ToString(),
                        MobilePhone = dtMembers.Rows[p]["Mobile_Phone"].ToString(),
                        HomePhone = dtMembers.Rows[p]["Home_Phone"].ToString(),
                        AddressLine1 = dtMembers.Rows[p]["Address_Line_1"].ToString(),
                        AddressLine2 = dtMembers.Rows[p]["Address_Line_2"].ToString(),
                        City = dtMembers.Rows[p]["City"].ToString(),
                        State = dtMembers.Rows[p]["State"].ToString(),
                        ZipCode = dtMembers.Rows[p]["Postal_Code"].ToString(),
                        ImagePath = ImagePath,
                        DomainGuid = DomainGuid,
                        FileName = dtMembers.Rows[p]["Unique_Name"].ToString() + "." + dtMembers.Rows[p]["Extension"].ToString()
                    });
                }
                for (int m = 0; m < dtMeetings.Rows.Count; m++)
                {
                    Meetings.Add(new Meeting
                    {
                        Id = int.Parse(dtMeetings.Rows[m]["Event_ID"].ToString()),
                        DisplayName = DateTime.Parse(dtMeetings.Rows[m]["Event_Start_Date"].ToString()).ToString("ddd, MMM d @ h:mmtt"),
                        EventTitle = dtMeetings.Rows[m]["Event_Title"].ToString(),
                        StartDate = DateTime.Parse(dtMeetings.Rows[m]["Event_Start_Date"].ToString()),
                        EndDate = DateTime.Parse(dtMeetings.Rows[m]["Event_End_Date"].ToString())
                    });
                }

            }
            data.Members = Members;
            data.Meetings = Meetings;
            return data;
        }

        public static List<GroupMember> GetEventGroupAttendees(int EventId, int GroupId)
        {
            List<GroupMember> Members = new List<GroupMember>();
            using (DataManager dm = new DataManager(PlatformUser.Current))
            {
                DataSet dsMyEventAttendees = dm.ExecuteProcedure("api_MGM_GetEventGroupAttendees", "EventID=" + EventId.ToString() + "&GroupID=" + GroupId.ToString() + "&CheckedInStatus=" + ConfigurationManager.AppSettings["CheckedInParticipationStatusId"]);
                for (int a = 0; a < dsMyEventAttendees.Tables[0].Rows.Count; a++)
                {
                    Members.Add(new GroupMember
                    {
                        Id = int.Parse(dsMyEventAttendees.Tables[0].Rows[a]["Group_Participant_ID"].ToString()),
                        ContactId = int.Parse(dsMyEventAttendees.Tables[0].Rows[a]["Contact_ID"].ToString()),
                        ParticipantId = int.Parse(dsMyEventAttendees.Tables[0].Rows[a]["Participant_ID"].ToString()),
                        GroupParticipantId = int.Parse(dsMyEventAttendees.Tables[0].Rows[a]["Group_Participant_ID"].ToString()),
                        DisplayName = dsMyEventAttendees.Tables[0].Rows[a]["Display_Name"].ToString(),
                        RoleTitle = dsMyEventAttendees.Tables[0].Rows[a]["Role_Title"].ToString(),
                        IsMeetingAttendee = int.Parse("0" + dsMyEventAttendees.Tables[0].Rows[a]["Event_Participant_ID"].ToString()) > 0 ? true : false
                    });
                }

            }
            return Members;
        }

        public static List<Note> GetMemberNotes(int UserId, int ContactId)
        {
            List<Note> Notes = new List<Note>();
            using (DataManager dm = new DataManager(PlatformUser.Current))
            {
                DataSet dsMyGroups = dm.ExecuteProcedure("api_MGM_GetContactNotes", "OwnerUserID=" + UserId.ToString() + "&TargetContactID=" + ContactId.ToString());
                for (int n = 0; n < dsMyGroups.Tables[0].Rows.Count; n++)
                {
                    Notes.Add(new Note
                    {
                        Id = int.Parse(dsMyGroups.Tables[0].Rows[n]["Contact_Log_ID"].ToString()),
                        Date = DateTime.Parse(dsMyGroups.Tables[0].Rows[n]["Contact_Date"].ToString()),
                        Notes = dsMyGroups.Tables[0].Rows[n]["Notes"].ToString()
                    });
                }
            }
            return Notes;
        }

        public static Meeting CreateMeeting(DateTime MeetingDate, int Duration, int CongregationId)
        {
            DataRecord NewEvent = new DataRecord("Events");
            NewEvent["Event_Title"].Value = ConfigurationManager.AppSettings["DefaultEventTitle"];
            NewEvent["Event_Type_ID"].Value = ConfigurationManager.AppSettings["DefaultEventTypeId"];
            NewEvent["Program_ID"].Value = ConfigurationManager.AppSettings["DefaultProgramId"];
            NewEvent["Congregation_ID"].Value = CongregationId;
            NewEvent["Primary_Contact"].Value = PlatformUser.Current.ContactId;
            NewEvent["Minutes_for_Setup"].Value = 0;
            NewEvent["Event_Start_Date"].Value = MeetingDate;
            NewEvent["Event_End_Date"].Value = MeetingDate.AddMinutes(Duration);
            NewEvent["Minutes_for_Cleanup"].Value = 0;
            NewEvent["Visibility_Level_ID"].Value = ConfigurationManager.AppSettings["DefaultEventVisibilityLevelId"];
            NewEvent["Ignore_Program_Groups"].Value = false;
            NewEvent["Prohibit_Guests"].Value = false;
            NewEvent["Allow_Check-in"].Value = false;
            NewEvent["On_Donation_Batch_Tool"].Value = false;

            List<DataError> Errors = NewEvent.Validate();
            NewEvent.Save();

            Meeting NewMeeting = new Meeting
            {
                Id = int.Parse(NewEvent["Event_ID"].Value.ToString()),
                EventTitle = NewEvent["Event_Title"].Value.ToString(),
                DisplayName = DateTime.Parse(NewEvent["Event_Start_Date"].Value.ToString()).ToString("ddd, MMM d @ h:mmtt"),
                StartDate = DateTime.Parse(NewEvent["Event_Start_Date"].Value.ToString()),
                EndDate = DateTime.Parse(NewEvent["Event_End_Date"].Value.ToString())
            };
            return NewMeeting;
        }

        public static void CreateEventGroup(int EventId, int GroupId)
        {
            DataRecord NewEventGroup = new DataRecord("Event_Groups");
            NewEventGroup["Event_ID"].Value = EventId;
            NewEventGroup["Group_ID"].Value = GroupId;
            //List<DataError> Errors = NewEventGroup.Validate();
            NewEventGroup.Save();

        }

        public static void CreateEventParticipant(int EventId, int ParticipantId, int GroupParticipantId, DateTime CheckinTime)
        {
            DataRecord NewEventParticipant = new DataRecord("Event_Participants");
            NewEventParticipant["Event_ID"].Value = EventId;
            NewEventParticipant["Participant_ID"].Value = ParticipantId;
            NewEventParticipant["Group_Participant_ID"].Value = GroupParticipantId;
            NewEventParticipant["Participation_Status_ID"].Value = ConfigurationManager.AppSettings["CheckedInParticipationStatusId"];
            NewEventParticipant["Time_In"].Value = CheckinTime;
            //List<DataError> Errors = NewEventParticipant.Validate();
            NewEventParticipant.Save();
        }

        public static int CreateOrUpdateNote(int NoteId, int ContactId, DateTime NoteDate, string Notes)
        {
            DataRecord Note = new DataRecord("Contact_Log", NoteId);
            Note["Contact_ID"].Value = ContactId;
            Note["Contact_Date"].Value = NoteDate;
            Note["Made_By"].Value = PlatformUser.Current.UserId;
            Note["Notes"].Value = Notes;
            //List<DataError> Errors = Note.Validate();
            Note.Save();
            return int.Parse(Note["Contact_Log_ID"].Value.ToString());
        }


    }
}