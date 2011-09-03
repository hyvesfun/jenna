using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Service
{
    public class CustomService:Service
    {
        public static void CustomHyvesFotosAppLaunched()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Request<bool>(HyvesMethod.CustomHyvesFotosAppLaunched, parameters, null, (RequestCallbackDelegate<bool>)delegate { /*ToDo: add login*/ });
        }

        public static void CustomHyvesFotosAlbumCreated()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Request<bool>(HyvesMethod.CustomHyvesFotosAlbumCreated, parameters, null, (RequestCallbackDelegate<bool>)delegate { /*ToDo: add login*/ });
        }

        public static void CustomHyvesFotosAppError(string message)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["message"] = message;
            Request<bool>(HyvesMethod.CustomHyvesFotosAppError, parameters, null, (RequestCallbackDelegate<bool>)delegate { /*ToDo: add login*/ });
        }

        public static void CustomHyvesFotosAppMediaUploaded(int mediaCount)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["mediacount"] = mediaCount.ToString();

            Request<bool>(HyvesMethod.CustomHyvesFotosAppMediaUploaded, parameters, null, (RequestCallbackDelegate<bool>)delegate { /*ToDo: add login*/ });
        }
    }
}