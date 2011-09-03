using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api.Model;

namespace Hyves.Api.Service
{

    public class HyvesRequestFactory
    {
        public static HyvesRequest<T> GetHyvesRequest<T>()
        {
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            return new HyvesRequest<T>(hyvesApplication.ConsumerKey, hyvesApplication.ConsumerSecret, hyvesApplication.HyvesHttpUri, hyvesApplication.ApiVersion);
        }

        public static HyvesRequest<T> GetHyvesRequestSecure<T>()
        {
            HyvesApplication hyvesApplication = HyvesApplication.GetInstance();
            return new HyvesRequest<T>(hyvesApplication.ConsumerKey, hyvesApplication.ConsumerSecret, hyvesApplication.HyvesHttpUriSecure, hyvesApplication.ApiVersion);
        }

        public static HyvesUploadRequest getHyvesUploadRequest(string filePath) 
        {
            return new HyvesUploadRequest(filePath);        
        }

        public static HyvesBatchUploadRequest GetHyvesBatchUploadRequest(List<string> filePathList, Album album)
        {
            return new HyvesBatchUploadRequest(filePathList, album);
        }
    }

}
