using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Hyves.Api.Model;

namespace Hyves.Api.Service
{
    public class HubService : Service
    {
        public static void HubGetByShortname(string shortname, HyvesServicesCallback<List<Hub>> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["shortname"] = shortname;
            parameters["ha_responsefields"] = "profilepicture";

            Request<List<Hub>>(HyvesMethod.HubsGetByShortname, parameters, serviceCallback, new RequestCallbackDelegate<List<Hub>>(HubGetByShortnameReponseCallback));
        }
        private static void HubGetByShortnameReponseCallback(RequestResult<List<Hub>> requestResult)
        {
            ServiceResult<List<Hub>> serviceResult = new ServiceResult<List<Hub>>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                HubGetByShortnameResponse gubGetByShortnameResponse = JsonConvert.DeserializeObject<HubGetByShortnameResponse>(requestResult.Response);
                serviceResult.Result = gubGetByShortnameResponse.hub;
            }
            requestResult.Callback(serviceResult);
        }
        private class HubGetByShortnameResponse
        {
            public List<Hub> hub { get; set; }
        }
    }
}
