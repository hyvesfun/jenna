using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Hyves.Api.Model;

namespace Hyves.Api.Service
{
    public class UserService : Service
    {
        public static void UsersGetByFriendsLastLogin(string userId, HyvesServicesCallback<List<User>> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["userid"] = userId;
            parameters["ha_responsefields"] = "profilepicture";
            
            Request<List<User>>(HyvesMethod.UsersGetByFriendLastlogin, parameters, 150, serviceCallback, new RequestCallbackDelegate<List<User>>(UsersGetByFriendsLastLoginReponseCallback));
        }
        private static void UsersGetByFriendsLastLoginReponseCallback(RequestResult<List<User>> requestResult)
        {
            ServiceResult<List<User>> serviceResult = new ServiceResult<List<User>>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                UsersGetByFriendsLastLoginResponse usersGetByFriendsLastLoginResponse = JsonConvert.DeserializeObject<UsersGetByFriendsLastLoginResponse>(requestResult.Response);
                serviceResult.Result = usersGetByFriendsLastLoginResponse.user;
            }
            requestResult.Callback(serviceResult);
        }
        private class UsersGetByFriendsLastLoginResponse
        {
            public List<User> user { get; set; }
        }
    }
}
