using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api;
using OAuth;
using System.Net;
using System.IO;
using System.Diagnostics;
using Hyves.Api.Model;
using Hyves.Api.Service;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Hyves.Api
{
    public class HyvesUploadRequest
    {
        private HttpWebRequest httpWebRequest;
        private HyvesServicesCallback<MediaToken> serviceCallback;
        private MediaToken mediaToken;
        private String filePath;

        public MediaToken MediaToken { get { return mediaToken; } }
        public string FilePath { get { return filePath; } }
        public HttpWebRequest HttpWebRequest
        {
            get { return httpWebRequest; }
        }
        public ServiceResult<MediaToken> ServiceResult { get; set; }
        public Media Media { get; set; }

        public HyvesUploadRequest(string filePath) 
        {
            this.filePath = filePath;
            this.mediaToken = new MediaToken();
            this.mediaToken.HyvesUploadRequest = this;
        }
        public void Upload(HyvesServicesCallback<MediaToken> serviceCallback) 
        {
            // Get Upload Token, Upload File, Start thread to check status(with timeout), Add media to Album
            this.serviceCallback = serviceCallback;
            MediaService.MediaGetUploadToken( new HyvesServicesCallback<MediaToken>(MediaTokenCallback));
        }
        private void MediaTokenCallback(ServiceResult<MediaToken> serviceResult) 
        {
            if (serviceResult.IsError) 
            {
                // calling consumer as error occurred
                serviceResult.Result = this.mediaToken;
                this.ServiceResult = serviceResult;
                this.serviceCallback(serviceResult);
                return;
            }
            // Uploading file
            this.mediaToken = serviceResult.Result;
            this.mediaToken.HyvesUploadRequest = this;
            UploadFileRequest(this.filePath, mediaToken);
        }
        private void UploadStatusRequest(MediaToken mediaToken)
        {
            // Initializing request
            string uploadUrl = string.Format("http://{0}/status?token={1}", mediaToken.ip, mediaToken.token);
            this.httpWebRequest = (HttpWebRequest)WebRequest.Create(uploadUrl);
            this.httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            httpWebRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), new RequestCallbackDelegate<string>(UploadFileStatusRequestCallback));
        }
        private void UploadFileRequest(string filePath, MediaToken mediaToken)
        {
            // Initializing request
            string uploadUrl = string.Format("http://{0}/upload?token={1}&name={2}", mediaToken.ip, mediaToken.token, Path.GetFileName(filePath));
            this.httpWebRequest = (HttpWebRequest)WebRequest.Create(uploadUrl);
            this.httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/octet-stream";
            httpWebRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), new RequestCallbackDelegate<string>(UploadFileRequestCallback));
        }
        private void UploadFileRequestCallback(RequestResult<string> requestResult) 
        {
            ServiceResult<MediaToken> serviceResult = new ServiceResult<MediaToken>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            serviceResult.Result = this.mediaToken;

            if (requestResult.IsError) 
            {
                // Calling Service callback to let consumer know
                this.ServiceResult = serviceResult;
                this.serviceCallback(serviceResult);
                return;
            }
            UploadStatusRequest(this.mediaToken);
        }
        private void UploadFileStatusRequestCallback(RequestResult<string> requestResult) 
        {
            // Using trick to get standard status, replacing token with "token_status" string
            JObject o = JObject.Parse(requestResult.Response);
            string status = o["data"][this.mediaToken.token][0].ToString();
            MediaTokenStatus mediaTokenStatus = JsonConvert.DeserializeObject<MediaTokenStatus>(status);
            this.mediaToken.MediaTokenStatus = mediaTokenStatus;

            ServiceResult<MediaToken> serviceResult = new ServiceResult<MediaToken>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
            serviceResult.Result = this.mediaToken;

            if(mediaTokenStatus.error.errornumber != null)
            {
                // Calling consumer 
                this.ServiceResult = serviceResult;
                this.serviceCallback(serviceResult);
                return;
            }

            if (mediaTokenStatus.currentstate == "done")
            {
                // Calling consumer
                this.ServiceResult = serviceResult;
                DetectFace();
                return;
            }
            // Wait for 1 sec
            System.Threading.Thread.Sleep(100);
            UploadStatusRequest(this.mediaToken);
        }
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                string filePath = this.filePath;
                byte[] bytes = getFileBytes(filePath);

                HttpWebRequest httpWebRequest = this.HttpWebRequest;

                // End the operation
                Stream postStream = httpWebRequest.EndGetRequestStream(asynchronousResult);

                // Write to the request stream.
                postStream.Write(bytes, 0, bytes.Length);
                postStream.Close();

                // Start the asynchronous operation to get the response
                httpWebRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), asynchronousResult.AsyncState);
            }
            catch (WebException e) 
            {
                RequestCallbackDelegate<string> callback = (RequestCallbackDelegate<string>)asynchronousResult.AsyncState;
                RequestResult<string> requestResult = new RequestResult<string>();
                requestResult.IsError = true;
                requestResult.Execption = e;
                requestResult.Message = e.Message;
                callback(requestResult);
            }
        }
        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            RequestCallbackDelegate<string> callback = (RequestCallbackDelegate<string>)asynchronousResult.AsyncState;
            RequestResult<string> requestResult = new RequestResult<string>();

            // End the operation
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)this.httpWebRequest.EndGetResponse(asynchronousResult);
            }
            catch (WebException e)
            {
                WebResponse r = e.Response;
                if (r != null)
                {
                    Stream s = r.GetResponseStream();
                    requestResult.Message = new StreamReader(s).ReadToEnd();
                }
                else 
                {
                    requestResult.Message = e.Message;
                }
                requestResult.IsError = true;
                requestResult.Execption = e;
                //Error occurred, calling callback
                callback(requestResult);
                return;
            }

            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            string responseString = streamRead.ReadToEnd();

            // Close the stream object
            streamResponse.Close();
            streamRead.Close();

            // Release the HttpWebResponse
            response.Close();

            //Calling callback
            requestResult.Response = responseString;
            requestResult.IsError = false;
            callback(requestResult);
        }

        private byte[] getFileBytes(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] bytes = new byte[fileStream.Length];
                int numBytesToRead = (int)fileStream.Length;
                int numBytesRead = 0;
                while (numBytesToRead > 0)
                {
                    // Read may return anything from 0 to numBytesToRead.
                    int n = fileStream.Read(bytes, numBytesRead, numBytesToRead);

                    // Break when the end of the file is reached.
                    if (n == 0)
                    {
                        break;
                    }
                    numBytesRead += n;
                    numBytesToRead -= n;
                }
                return bytes;
            }

        }

        public void DetectFace() 
        {
            MediaService.MediaGet(mediaToken.MediaTokenStatus.done.mediaid, (HyvesServicesCallback<List<Media>>)delegate(ServiceResult<List<Media>> requestResult)
            {
                Media = requestResult.Result[0];
                Media.square_extralarge.image = Util.GetImageFromUrl(Media.square_extralarge.src);
                if (Media.mediatype == "image")
                {
                    Media.image.image = Util.GetImageFromUrl(Media.image.src);

                     try
                    {
                        Image<Bgr, Byte> image = new Image<Bgr, byte>(new Bitmap(Media.image.image)); //Read the files as an 8-bit Bgr image  
                        Image<Gray, Byte> gray = image.Convert<Gray, Byte>(); //Convert it to Grayscale

                        //normalizes brightness and increases contrast of the image
                        gray._EqualizeHist();

                        //Read the HaarCascade objects
                        HaarCascade face = new HaarCascade("haarcascade_frontalface_alt2.xml");

                        //Detect the faces  from the gray scale image and store the locations as rectangle
                        //The first dimensional is the channel
                        //The second dimension is the index of the rectangle in the specific channel
                        MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
                           face,
                           1.1,
                           10,
                           Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DEFAULT,
                           new Size(10, 10));

                        Media.Faces = facesDetected;
                        
                    }
                    catch (Exception ex) {
                        //ToDo: Add error handling
                    }
                    
                    ServiceResult<MediaToken> serviceResult = new ServiceResult<MediaToken>() { IsError = requestResult.IsError, Execption = requestResult.Execption, Message = requestResult.Message };
                    serviceResult.Result = this.mediaToken;
                    this.ServiceResult = serviceResult;
                    this.serviceCallback(serviceResult);
                }
            });
        }
    }
}
