using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api;
using Hyves.Api.Model;
using Newtonsoft.Json;

namespace Hyves.Api.Service
{
    public delegate void RequestTokenCallback(RequestToken requestToken);
    public delegate void AccessTokenCallback(AccessToken accessToken);

    public class AuthService:Service
    {
        public static void RequestToken(HyvesServicesCallback<RequestToken> serviceCallback)
        {
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["methods"] = GetMethods(HyvesMethod.All);
            
            HyvesRequest<RequestToken> hyvesRequest = HyvesRequestFactory.GetHyvesRequest<RequestToken>();
            RequestResult<RequestToken> requestResult = new RequestResult<RequestToken>() { Callback = serviceCallback };
            hyvesRequest.Request(HyvesMethod.AuthRequesttoken, parameters, "", "", new RequestCallbackDelegate<RequestToken>(RequestTokenReponseCallback), requestResult);

        }
        private static void RequestTokenReponseCallback(RequestResult<RequestToken> requestResult)
        {
            ServiceResult<RequestToken> serviceResult = new ServiceResult<RequestToken>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                RequestToken requestToken = JsonConvert.DeserializeObject<RequestToken>(requestResult.Response);
                serviceResult.Result = requestToken;
            }
            requestResult.Callback(serviceResult);
        }

        public static void AccessToken(HyvesServicesCallback<AccessToken> serviceCallback)
        {
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            HyvesRequest<AccessToken> hyvesRequest = HyvesRequestFactory.GetHyvesRequest<AccessToken>();
            RequestResult<AccessToken> requestResult = new RequestResult<AccessToken>() { Callback = serviceCallback };
            hyvesRequest.Request(HyvesMethod.AuthAccesstoken, parameters, hyvesApplication.RequestToken, hyvesApplication.RequestTokenSecret, new RequestCallbackDelegate<AccessToken>(AccessTokenResponseCallback), requestResult);

        }
        private static void AccessTokenResponseCallback(RequestResult<AccessToken> requestResult)
        {
            ServiceResult<AccessToken> serviceResult = new ServiceResult<AccessToken>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                AccessToken requestToken = JsonConvert.DeserializeObject<AccessToken>(requestResult.Response);
                serviceResult.Result = requestToken;
            }
            requestResult.Callback(serviceResult);
        }

        public static void login(string username, string password, HyvesServicesCallback<RequestToken> serviceCallback) 
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["username"] = username;
            parameters["userpassword"] = password;

            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            HyvesRequest<RequestToken> hyvesRequest = HyvesRequestFactory.GetHyvesRequestSecure<RequestToken>();
            RequestResult<RequestToken> requestResult = new RequestResult<RequestToken>() { Callback = serviceCallback };
            hyvesRequest.Request(HyvesMethod.AuthLogin, parameters,"", "", new RequestCallbackDelegate<RequestToken>(LoginReponseCallback), requestResult);
        }

        private static void LoginReponseCallback(RequestResult<RequestToken> requestResult)
        {
            ServiceResult<RequestToken> serviceResult = new ServiceResult<RequestToken>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                RequestToken requestToken = JsonConvert.DeserializeObject<RequestToken>(requestResult.Response);
                serviceResult.Result = requestToken;
            }
            requestResult.Callback(serviceResult);
        }

        private static string GetMethods(HyvesMethod hyvesMethod)
        {
            //ToDo: fix hyves method to be sync with hyves api 2.0 methods  
            return "albums.getByUser,media.getByAlbum";
            StringBuilder methods = new StringBuilder();
            // Getting string for all methods
            if (hyvesMethod == HyvesMethod.All)
            {
                Array hyvesMethodValues = Enum.GetValues(typeof(HyvesMethod));
                foreach (HyvesMethod method in hyvesMethodValues)
                {
                    if (method != HyvesMethod.Unknown && method != HyvesMethod.All)
                    {
                        methods.Append(string.Format("{0},", EnumHelper.GetDescription(method)));
                    }
                }
                return methods.ToString();
            }

            // String for one method
            return EnumHelper.GetDescription(hyvesMethod);
        }
    }
}
