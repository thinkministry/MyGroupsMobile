using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Configuration;
using MyGroups.Models;
using PlatformSDK;

namespace MyGroups.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            LogonError NoError = new LogonError() { ErrorMessage = "Please enter a valid user name and password" };
            return View(NoError);
        }

        [HttpPost]
        public ActionResult Authenticate(string UserName, string Password, string ReturnUrl)
        {
            int SecurityRoleId = int.Parse(ConfigurationManager.AppSettings["SecurityRoleId"]);
            PlatformUser User = new PlatformUser(UserName, Password, SecurityRoleId);

            if (User.UserId > 0)
            {
                FormsAuthentication.SetAuthCookie(UserName, false);
                return RedirectToAction("Index", "Groups");
            }
            else
            {
                LogonError ErrorFound = new LogonError() { ErrorMessage = "Login was unsuccessful. Please try again." };
                return View("Index", ErrorFound);
            }

        }
    }
}
