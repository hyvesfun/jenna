using System;
using System.Net;
using System.Collections.Generic;
using System.Text;
using OAuth;
using System.IO;
using System.Diagnostics;
using Hyves.Api;
using Hyves.Api.Service;
using Hyves.Api.Model;
using System.Configuration;

namespace Hyves.Api
{

    [Serializable]
    public class HyvesApplication
    {
        private static HyvesApplication hyvesApplication = null;
        private static object _lock = new Object();

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }

        public string HyvesHttpUri { get; set; }
        public string HyvesHttpUriSecure { get; set; }
        public string ApiVersion { get; set; }

        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }

        public string RequestToken { get; set; }
        public string RequestTokenSecret { get; set; }

        private string GetLoggedInUserId { get; set; }
        public string UserId { get; set; }

        public Version Version { get; set; }

        private HyvesApplication()
        {
            ConsumerKey = System.Configuration.ConfigurationManager.AppSettings["ConsumerKey"];
            ConsumerSecret = System.Configuration.ConfigurationManager.AppSettings["ConsumerSecret"];

            HyvesHttpUri = System.Configuration.ConfigurationManager.AppSettings["HyvesHttpUri"];
            HyvesHttpUriSecure = System.Configuration.ConfigurationManager.AppSettings["HyvesHttpUriSecure"];
            ApiVersion = System.Configuration.ConfigurationManager.AppSettings["ApiVersion"];
        }

        public static HyvesApplication GetInstance(HyvesApplication hyvesApplication) 
        {
            lock (_lock)
            {
                if (HyvesApplication.hyvesApplication == null)
                {
                    HyvesApplication.hyvesApplication = hyvesApplication;
                }
                else
                {
                    throw new Exception("Hyves application is aready initialized");
                }
            }
            return HyvesApplication.hyvesApplication;
        }

        public static HyvesApplication GetInstance()
        {
            lock (_lock)
            {
                if (hyvesApplication == null)
                {
                    hyvesApplication = new HyvesApplication();
                }
            }
            return hyvesApplication;
        }

        public bool LoginIn()
        {
            LoginForm loginForm = new LoginForm();
            return loginForm.ShowDialog() == System.Windows.Forms.DialogResult.OK;
        }

        public void LoginIn(string username, string password, HyvesServicesCallback<bool> loginCallbackDelegate) 
        {
            LoginNoPopup loginNoPopup = new LoginNoPopup(loginCallbackDelegate);
            loginNoPopup.LoginIn(username, password);
        }
    }
}
