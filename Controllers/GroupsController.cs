using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MyGroups.Models;
using MyGroups.Data;
using PlatformSDK;

namespace MyGroups.Controllers
{
    public class GroupsController : Controller
    {

        [Authorize]
        public ActionResult Index()
        {
            Main MainModel = new Main();

            MainModel.CurrentUser = PlatformUser.Current;
            MainModel.Groups = Translator.GetMyGroups();
            return View(MainModel);
        }

        [Authorize]
        public JsonResult GetMembersAndMeetings(int GroupId)
        {
            MembersAndMeetings MembersModel = Translator.GetMyGroupMembersAndMeetings(GroupId);
            return Json(MembersModel);
        }

        [Authorize]
        public JsonResult GetMeetingAttendees(int EventId, int GroupId)
        {
            List<GroupMember> Attendees = Translator.GetEventGroupAttendees(EventId, GroupId);
            return Json(Attendees);
        }

        [Authorize]
        public JsonResult GetMemberNotes(int ContactId)
        {
            List<Note> Notes = Translator.GetMemberNotes(PlatformUser.Current.UserId, ContactId);
            return Json(Notes);
        }

    }
}
