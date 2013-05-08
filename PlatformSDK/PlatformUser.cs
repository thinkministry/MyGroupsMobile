using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Data;
using System.Configuration;

namespace PlatformSDK
{
    public class PlatformUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int ContactId { get; set; }
        public int DomainId { get; set; }
        public string DisplayName { get; set; }
        public string ContactEmail { get; set; }
        public string DomainGuid { get; set; }
        public string UserGuid { get; set; }
        public bool Authorized { get; set; }

        private Platform.apiSoapClient _service;


        public PlatformUser(string UserName, string Password, int SecurityRoleId)
        {
            this.UserId = 0;
            this.Authorized = false;

            string serverName = ConfigurationManager.AppSettings["ServerName"] != "" ? ConfigurationManager.AppSettings["ServerName"] : HttpContext.Current.Request.ServerVariables["SERVER_NAME"];
            string apiPassword = ConfigurationManager.AppSettings["APIPassword"];

            int userId, contactId, domainId;
            string domainGuid, userGuid, displayName, contactEmail, externalUrl;
            bool canImpersonate;

            _service = new Platform.apiSoapClient();
            _service.Open();
            _service.AuthenticateUser(UserName, Password, serverName, out userId, out contactId, out domainId,
                out domainGuid, out userGuid, out displayName, out contactEmail, out externalUrl, out canImpersonate);

            if (userId > 0 && domainId > 0)
            {
                DataSet dsRoles = _service.ExecuteStoredProcedure(domainGuid, apiPassword, "api_SDK_GetUserRoles", "UserID=" + userId.ToString());
                for (int r = 0; r < dsRoles.Tables[0].Rows.Count; r++)
                {
                    if (int.Parse(dsRoles.Tables[0].Rows[r]["Role_ID"].ToString()) == SecurityRoleId)
                    {
                        this.Authorized = true;
                        break;
                    }
                }
            }
            _service.Close();
            _service = null;

            if (userId > 0 && domainId > 0 && this.Authorized == true)
            {
                this.UserId = userId;
                this.UserName = UserName;
                this.ContactId = contactId;
                this.ContactEmail = contactEmail;
                this.DisplayName = displayName;
                this.DomainId = domainId;
                this.DomainGuid = domainGuid;
                this.UserGuid = userGuid;

                HttpContext.Current.Session["User"] = this;
            }

        }

        public PlatformUser(string UserGUID, string DomainGUID)
        {
            this.UserId = 0;

            int userId, contactId, domainId;
            string userName, displayName, contactEmail, externalUrl;
            bool canImpersonate;

            _service = new Platform.apiSoapClient();
            _service.Open();

            _service.AuthenticateGUIDS(UserGUID, DomainGUID, out userId, out userName, out contactId, out domainId,
                out displayName, out contactEmail, out externalUrl, out canImpersonate);

            _service.Close();
            _service = null;

            if (userId > 0 && domainId > 0)
            {
                this.UserId = userId;
                this.UserName = userName;
                this.ContactId = contactId;
                this.ContactEmail = contactEmail;
                this.DisplayName = displayName;
                this.DomainId = domainId;
                this.DomainGuid = DomainGUID;
                this.UserGuid = UserGUID;

                HttpContext.Current.Session["User"] = this;
            }
        }
        /// <summary>
        ///     Gets currenly logged-in web user object.
        /// </summary>
        public static PlatformUser Current
        {
            get
            {
                if (HttpContext.Current == null ||
                    HttpContext.Current.Session == null)
                {
                    return null;
                }

                return HttpContext.Current.Session["User"] as PlatformUser;
            }
        }
    }
}
