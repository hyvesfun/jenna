using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api.Model;
using Hyves.Api;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Hyves.Api.Service
{
    public class MediaService: Service
    {
        public static void AlbumsGetByUser(string userId, HyvesServicesCallback<List<Album>> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["userid"] = userId;
            Request<List<Album>>(HyvesMethod.AlbumsGetByUser, parameters, serviceCallback, new RequestCallbackDelegate<List<Album>>(AlbumsGetByUserReponseCallback));
        }
        private static void AlbumsGetByUserReponseCallback(RequestResult<List<Album>> requestResult)
        {
            ServiceResult<List<Album>> serviceResult = new ServiceResult<List<Album>>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                AlbumsGetByUserResponse albumsGetByUserResponse = JsonConvert.DeserializeObject<AlbumsGetByUserResponse>(requestResult.Response);
                serviceResult.Result = albumsGetByUserResponse.album;
            }
            requestResult.Callback(serviceResult);
        }

        public static void AlbumsGetByHub(string hubId, HyvesServicesCallback<List<Album>> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["hubid"] = hubId;
            Request<List<Album>>(HyvesMethod.AlbumsGetByHub, parameters, serviceCallback, new RequestCallbackDelegate<List<Album>>(AlbumsGetByHubReponseCallback));
        }
        private static void AlbumsGetByHubReponseCallback(RequestResult<List<Album>> requestResult)
        {
            ServiceResult<List<Album>> serviceResult = new ServiceResult<List<Album>>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                AlbumsGetByUserResponse albumsGetByUserResponse = JsonConvert.DeserializeObject<AlbumsGetByUserResponse>(requestResult.Response);
                serviceResult.Result = albumsGetByUserResponse.album;
            }
            requestResult.Callback(serviceResult);
        }

        public static void MediaGetByAlbum(string albumId, HyvesServicesCallback<List<Media>> serviceCallback) 
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["albumid"] = albumId;
            Request<List<Media>>(HyvesMethod.MediaGetByAlbum, parameters, 500, serviceCallback, new RequestCallbackDelegate<List<Media>>(MediaGetByReponseCallback));
        }
        private static void MediaGetByReponseCallback(RequestResult<List<Media>> requestResult)
        {
            ServiceResult<List<Media>> serviceResult = new ServiceResult<List<Media>>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                MediaGetByUserResponse mediaGetByUserResponse = JsonConvert.DeserializeObject<MediaGetByUserResponse>(requestResult.Response);
                serviceResult.Result = mediaGetByUserResponse.media;
            }
            requestResult.Callback(serviceResult);
        }

        public static void MediaGet(string mediaIds, HyvesServicesCallback<List<Media>> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["mediaid"] = mediaIds;
            Request<List<Media>>(HyvesMethod.MediaGet, parameters, serviceCallback, new RequestCallbackDelegate<List<Media>>(MediaGetReponseCallback));
        }
        private static void MediaGetReponseCallback(RequestResult<List<Media>> requestResult)
        {
            ServiceResult<List<Media>> serviceResult = new ServiceResult<List<Media>>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                MediaGetByUserResponse mediaGetByUserResponse = JsonConvert.DeserializeObject<MediaGetByUserResponse>(requestResult.Response);
                serviceResult.Result = mediaGetByUserResponse.media;
            }
            requestResult.Callback(serviceResult);
        }

        public static void MediaGetUploadToken(HyvesServicesCallback<MediaToken> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            Request<MediaToken>(HyvesMethod.MediaGetUploadToken, parameters, serviceCallback, new RequestCallbackDelegate<MediaToken>(MediaGetUploadTokenReponseCallback));
        }
        private static void MediaGetUploadTokenReponseCallback(RequestResult<MediaToken> requestResult)
        {
            ServiceResult<MediaToken> serviceResult = new ServiceResult<MediaToken>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                MediaToken mediaToken = JsonConvert.DeserializeObject<MediaToken>(requestResult.Response);
                serviceResult.Result = mediaToken;
            }
            requestResult.Callback(serviceResult);
        }

        public static void UploadFile(string filePath, HyvesServicesCallback<MediaToken> servicesCallback) 
        {
            HyvesUploadRequest hyvesUploadRequest = HyvesRequestFactory.getHyvesUploadRequest(filePath);
            hyvesUploadRequest.Upload(servicesCallback);
            //ToDo:: add subscriber for uploadStatsChanged events
        }

        public static void AlbumAddMedia(Album album, List<string> mediaIdList, HyvesServicesCallback<bool> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["albumid"] = album.albumid;
            StringBuilder sbMediaList = new StringBuilder();
            foreach (String mediaId in mediaIdList)
            {
                sbMediaList.Append(mediaId);
                sbMediaList.Append(",");
            }
            // Removing last comma
            if (sbMediaList.Length > 0)
            {
                sbMediaList.Remove(sbMediaList.Length - 1, 1);
            }
            parameters["mediaid"] = sbMediaList.ToString();
            Request<bool>(HyvesMethod.AlbumsAddMedia, parameters, serviceCallback, new RequestCallbackDelegate<bool>(AlbumAddMediaReponseCallback));
        }
        public static void AlbumAddMedia(Album album, List<Media> mediaList, HyvesServicesCallback<bool> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["albumid"] = album.albumid;
            StringBuilder sbMediaList = new StringBuilder();
            foreach (Media media in mediaList)
            {
                sbMediaList.Append(media.mediaid);
                sbMediaList.Append(",");
            }
            // Removing last comma
            sbMediaList.Remove(sbMediaList.Length - 1, 1);
            parameters["mediaid"] = sbMediaList.ToString();

            Request<bool>(HyvesMethod.AlbumsAddMedia, parameters, serviceCallback, new RequestCallbackDelegate<bool>(AlbumAddMediaReponseCallback));
        }
        private static void AlbumAddMediaReponseCallback(RequestResult<bool> requestResult)
        {
            ServiceResult<bool> serviceResult = new ServiceResult<bool>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                serviceResult.Result = true;
            }
            requestResult.Callback(serviceResult);
        }

        public static HyvesBatchUploadRequest UploadFiles(List<string> filePathList, Album album, HyvesServicesCallback<HyvesBatchUploadRequest> hyvesBatchUploadCallback)
        {
            HyvesBatchUploadRequest hyvesBatchUploadRequest = HyvesRequestFactory.GetHyvesBatchUploadRequest(filePathList, album);
            hyvesBatchUploadRequest.Upload(hyvesBatchUploadCallback);
            return hyvesBatchUploadRequest;
        }

        public static void AlbumsCreate(string title, Visibility visibility, HyvesServicesCallback<Album> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters["title"] = title;
            parameters["visibility"] = EnumHelper.GetDescription(visibility);
            parameters["printability"] = EnumHelper.GetDescription(visibility);

            Request<Album>(HyvesMethod.AlbumsCreate, parameters, serviceCallback, new RequestCallbackDelegate<Album>(AlbumsCreateReponseCallback));
        }
        private static void AlbumsCreateReponseCallback(RequestResult<Album> requestResult)
        {
            ServiceResult<Album> serviceResult = new ServiceResult<Album>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                AlbumsGetByUserResponse albumsGetByUserResponse = JsonConvert.DeserializeObject<AlbumsGetByUserResponse>(requestResult.Response);
                serviceResult.Result = albumsGetByUserResponse.album[0];
            }
            requestResult.Callback(serviceResult);
        }


        public static void MediaAddSpotted(string mediaId, string userId, MediaSpottedRectangle rectangle, HyvesServicesCallback<bool> serviceCallback)
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            parameters["mediaid"] = mediaId;
            parameters["target_userid"] = userId;
            parameters["rectangle"] = string.Format("{0},{1},{2},{3}", rectangle.x, rectangle.y, rectangle.width, rectangle.height);
            Request<bool>(HyvesMethod.MediaAddSpotted, parameters, serviceCallback, new RequestCallbackDelegate<bool>(MediaAddSpottedReponseCallback));
        }
        private static void MediaAddSpottedReponseCallback(RequestResult<bool> requestResult)
        {
            ServiceResult<bool> serviceResult = new ServiceResult<bool>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            if (!requestResult.IsError)
            {
                serviceResult.Result = true;
            }
            requestResult.Callback(serviceResult);
        }

        private class AlbumsGetByUserResponse
        {
            public List<Album> album { get; set; }
        }
        private class MediaGetByUserResponse
        {
            public List<Media> media {get; set;}
        }
    }
}
