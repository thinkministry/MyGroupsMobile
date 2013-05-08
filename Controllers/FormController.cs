using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyGroups.Data;
using MyGroups.Models;

namespace MyGroups.Controllers
{
    public class FormController : Controller
    {

        [HttpPost]
        [Authorize]
        public JsonResult CreateMeeting(int GroupId, int CongregationId, DateTime NewMeetingDate, int NewMeetingHour, int NewMeetingMinute, string NewMeetingAMPM, int NewMeetingDuration)
        {
            DateTime MeetingDate = DateTime.Parse(NewMeetingDate.ToShortDateString() + " " + NewMeetingHour.ToString() + ":" + NewMeetingMinute.ToString() + " " + NewMeetingAMPM);

            Meeting NewMeeting = Translator.CreateMeeting(MeetingDate, NewMeetingDuration, CongregationId);
            Translator.CreateEventGroup(NewMeeting.Id, GroupId);

            return Json(NewMeeting);
        }

        [HttpPost]
        [Authorize]
        public JsonResult CheckInMember(int EventId, int[] ParticipantIds, int[] GroupParticipantIds, DateTime LocalTime) //int[][] SelectedMemberData
        {
            for (int p = 0; p < ParticipantIds.Length; p++)
            {
                Translator.CreateEventParticipant(EventId, ParticipantIds[p], GroupParticipantIds[p], LocalTime);
            }
            return Json(new { success = true });
        }

        [HttpPost]
        [Authorize]
        public JsonResult CreateUpdateNote(int NoteId, int ContactId, string NoteText, DateTime LocalTime)
        {

            int Id = Translator.CreateOrUpdateNote(NoteId, ContactId, LocalTime, NoteText);

            Note NewNote = new Note
            {
                Id = Id,
                Date = LocalTime,
                Notes = NoteText
            };

            return Json(NewNote);
        }

    }
}
