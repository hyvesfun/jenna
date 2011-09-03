using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api.Model;

namespace Hyves.Api.Service
{
    public class Service
    {
        protected static void Request<T>(HyvesMethod hyvesMethod, Dictionary<string, string> parameters, HyvesServicesCallback<T> serviceCallback, RequestCallbackDelegate<T> requestCallback)
        {
            Request<T>(hyvesMethod, parameters, -1, serviceCallback, requestCallback);
        }
        protected static void Request<T>(HyvesMethod hyvesMethod, Dictionary<string, string> parameters, int pageSize, HyvesServicesCallback<T> serviceCallback, RequestCallbackDelegate<T> requestCallback)
        {
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            HyvesRequest<T> hyvesRequest = HyvesRequestFactory.GetHyvesRequest<T>();
            RequestResult<T> requestResult = new RequestResult<T>() { Callback = serviceCallback };
            hyvesRequest.Request(hyvesMethod, parameters, hyvesApplication.AccessToken, hyvesApplication.AccessTokenSecret, false, 0, pageSize, requestCallback, requestResult);
        }
    }
}
