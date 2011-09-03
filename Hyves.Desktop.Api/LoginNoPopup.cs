using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api.Service;
using Hyves.Api.Model;

namespace Hyves.Api
{
    public class LoginNoPopup
    {
        private HyvesServicesCallback<bool> loginCallbackDelegate;
        public LoginNoPopup(HyvesServicesCallback<bool> loginCallbackDelegate)
        {
            this.loginCallbackDelegate = loginCallbackDelegate;
        }
        public void LoginIn(string username, string password)
        {
            // Getting Authorized Request Token
            AuthService.login(username, password, new HyvesServicesCallback<RequestToken>(RequestTokenCallback));
        }

        public void RequestTokenCallback(ServiceResult<RequestToken> serviceResult)
        {
            if (serviceResult.IsError) 
            {
                ServiceResult<bool> result = new ServiceResult<bool>() { IsError = serviceResult.IsError, Execption = serviceResult.Execption, Message = serviceResult.Message };
                result.Result = false;
                loginCallbackDelegate(result);
                return;
            }

            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            hyvesApplication.RequestToken = serviceResult.Result.oauth_token;
            hyvesApplication.RequestTokenSecret = serviceResult.Result.oauth_token_secret;

            // Getting Access token
            AuthService.AccessToken(new HyvesServicesCallback<AccessToken>(AccessTokenCallback));
        }

        private void AccessTokenCallback(ServiceResult<AccessToken> serviceResult)
        {
            ServiceResult<bool> result = new ServiceResult<bool>() { IsError = serviceResult.IsError, Execption = serviceResult.Execption, Message = serviceResult.Message };
            if (serviceResult.IsError)
            {
                result.Result = false;
                loginCallbackDelegate(result);
                return;
            }

            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            hyvesApplication.AccessToken = serviceResult.Result.oauth_token;
            hyvesApplication.AccessTokenSecret = serviceResult.Result.oauth_token_secret;
            hyvesApplication.UserId = serviceResult.Result.userid;

            result.Result = true;
            // Letting user interface know
            loginCallbackDelegate(result);
        }
    }
}
