using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api.Model;
using System.Collections;
using System.Diagnostics;

namespace Hyves.Api.Service
{
    public class HyvesBatchUploadRequest
    {
        private object _lock = new object();

        private int uploadCompleted = 0;
        private HyvesServicesCallback<HyvesBatchUploadRequest> hyvesBatchUploadCallback;
        private Album album;
        private List<string> filePathList;
        private Dictionary<HyvesUploadRequest, bool> hyvesUploadRequestList = new Dictionary<HyvesUploadRequest, bool>();
        private Queue<HyvesUploadRequest> hyvesUploadRequestQueue = new Queue<HyvesUploadRequest>();

        public bool Cancel { get; set; }
        public int ParallelUpload { get; set; }
        public Album Album { get{return album;} }
        public Dictionary<HyvesUploadRequest, bool> HyvesUploadRequestList { get { return hyvesUploadRequestList; } }

        public event HyvesServicesCallback<int> OnBatchUploadProgressChanged;

        public HyvesBatchUploadRequest(List<string> filePathList, Album album) 
        {
            this.album = album;
            this.filePathList = filePathList;

            foreach (string filePath in filePathList) 
            {
                HyvesUploadRequest hyvesUploadRequest = new HyvesUploadRequest(filePath);
                hyvesUploadRequestQueue.Enqueue(hyvesUploadRequest);
            }
            this.Cancel = false;
            this.ParallelUpload = 10;
        }
        public void Upload(HyvesServicesCallback<HyvesBatchUploadRequest> hyvesBatchUploadCallback)
        {
            this.hyvesBatchUploadCallback = hyvesBatchUploadCallback;
            for (int count = 0; count < ParallelUpload; count++)
            {
                if (hyvesUploadRequestQueue.Count > 0)
                {
                    HyvesUploadRequest hyvesUploadRequest = hyvesUploadRequestQueue.Dequeue();
                    hyvesUploadRequestList.Add(hyvesUploadRequest, false);
                    hyvesUploadRequest.Upload(new HyvesServicesCallback<MediaToken>(UploadFileCallback));
                }
            }
        }
        private void UploadFileCallback(ServiceResult<MediaToken> serviceResult)
        {
            hyvesUploadRequestList[serviceResult.Result.HyvesUploadRequest] = true;
            lock (_lock)
            {
                uploadCompleted++;
                if (this.hyvesUploadRequestQueue.Count > 0 && !Cancel)
                {
                    HyvesUploadRequest hyvesUploadRequest = hyvesUploadRequestQueue.Dequeue();
                    hyvesUploadRequestList.Add(hyvesUploadRequest, false);
                    hyvesUploadRequest.Upload(new HyvesServicesCallback<MediaToken>(UploadFileCallback));
                }
                else
                {
                    bool isAllProcessed = true;
                    foreach (KeyValuePair<HyvesUploadRequest, bool> status in hyvesUploadRequestList)
                    {
                        // checking if all uploads are finished
                        if (!status.Value)
                        {
                            isAllProcessed = false;
                            break;
                        }
                    }
                    if (isAllProcessed)
                    {
                        AddMediaToAlbum();
                    }
                }
            }

            
            // Reporting progress
            if (OnBatchUploadProgressChanged != null)
            {
                ServiceResult<int> result = new ServiceResult<int>() { IsError = serviceResult.IsError, Execption = serviceResult.Execption, Message = serviceResult.Message };
                result.Result = uploadCompleted;
                OnBatchUploadProgressChanged(result);
            }
        }

        private void AddMediaToAlbum() 
        {
            List<string> mediaIdList = new List<string>();
            foreach (KeyValuePair<HyvesUploadRequest, bool> status in hyvesUploadRequestList)
            {
                if (!status.Key.ServiceResult.IsError && status.Key.MediaToken.MediaTokenStatus.done.mediaid != null)
                {
                    mediaIdList.Add(status.Key.MediaToken.MediaTokenStatus.done.mediaid);
                }
            }
            MediaService.AlbumAddMedia(this.album, mediaIdList, new HyvesServicesCallback<bool>(AddMediaToAlbumCallback));
        }

        private void AddMediaToAlbumCallback(ServiceResult<bool> serviceResult) 
        {
            ServiceResult<HyvesBatchUploadRequest> result = new ServiceResult<HyvesBatchUploadRequest>() { IsError = serviceResult.IsError, Execption = serviceResult.Execption, Message = serviceResult.Message };
            result.Result = this;
            if (!serviceResult.IsError)
            {
                foreach (KeyValuePair<HyvesUploadRequest, bool> status in hyvesUploadRequestList)
                {
                    if (!status.Key.ServiceResult.IsError) 
                    {
                        this.album.mediacount++;    
                    }
                }
            }
            this.hyvesBatchUploadCallback(result);
        }
    }
}
