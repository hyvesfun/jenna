using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Hyves.Api;
using Newtonsoft.Json;
using Hyves.Api.Model;
using Hyves.Api.Service;

namespace Hyves.Api
{
    public partial class LoginForm : Form
    {
        private delegate void StartLoginDelegate(string oauth_token);

        //ToDo : Move to application settings
        private const string LoginUrlFormat = @"http://www.hyves.nl/api/authorize/?oauth_token={0}&infinite=true&callback_url=http://www.hyves.nl/";
        private const string LoadingHtml = @"<span style=""font-family: tahoma; font-size: 8pt"">Loading...</span>";
        private const string ApiAccepted = @"http://www.hyves.nl/api/accepted";

        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            AuthService.RequestToken(new HyvesServicesCallback<RequestToken>(RequestTokenCallback));
        }

        private void RequestTokenCallback(ServiceResult<RequestToken> serviceResult)
        {
            StartLoginDelegate startLoginDelegate = new StartLoginDelegate(StartLogin);
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            hyvesApplication.RequestToken = serviceResult.Result.oauth_token;
            hyvesApplication.RequestTokenSecret = serviceResult.Result.oauth_token_secret;

            // Showing LoginForm to user
            this.BeginInvoke(startLoginDelegate, serviceResult.Result.oauth_token);

        }
        private void StartLogin(string oauth_token)
        {
            string loginUrl = String.Format(LoginUrlFormat, oauth_token);
            browser.Navigate(new Uri(loginUrl));
        }

        private void browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            if (e.Url.AbsoluteUri.StartsWith(ApiAccepted))
            {
                // Lets get access token
                AuthService.AccessToken(new HyvesServicesCallback<AccessToken>(AccessTokenCallback));
            }
        }
        private void AccessTokenCallback(ServiceResult<AccessToken> serviceResult)
        {
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            hyvesApplication.AccessToken = serviceResult.Result.oauth_token;
            hyvesApplication.AccessTokenSecret = serviceResult.Result.oauth_token_secret;
            hyvesApplication.UserId = serviceResult.Result.userid;
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
