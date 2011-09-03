using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hyves.Api;
using OAuth;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Hyves.Api.Model;

namespace Hyves.Api.Service
{
    public class HyvesRequest<T>
    {
        private OAuthBase oAuthBase = new OAuthBase();
        private HttpWebRequest httpWebRequest;

        private RequestCallbackDelegate<T> requestCallback;
        private Dictionary<string, string> parameters;

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string HyvesHttpUri { get; set; }
        public string ApiVersion { get; set; }


        public HttpWebRequest HttpWebRequest
        {
            get { return httpWebRequest; }
        }
        public Dictionary<string, string> Parameters
        {
            get { return parameters; }
        }
        public RequestCallbackDelegate<T> RequestCallback
        {
            get { return requestCallback; }
        }

        public RequestResult<T> RequestResult { get; set; }

        public HyvesRequest(string consumerKey, string consumerSecret, string hyvesHttpUri, string apiVersion)
        {
            this.ConsumerKey = consumerKey;
            this.ConsumerSecret = consumerSecret;
            this.HyvesHttpUri = hyvesHttpUri;
            this.ApiVersion = apiVersion;
        }

        public void Request(HyvesMethod method, Dictionary<string, string> parameters, string token, string tokenSecret, RequestCallbackDelegate<T> requestCallback, RequestResult<T> requestResult)
        {
            Request(method, parameters, token, tokenSecret, false, -1, -1, requestCallback, requestResult);
        }

        public void Request(HyvesMethod method, Dictionary<string, string> parameters, string token, string tokenSecret, bool useFancyLayout, int page, int resultsPerPage, RequestCallbackDelegate<T> requestCallback, RequestResult<T> requestResult)
        {
            // Initializing request
            this.httpWebRequest = (HttpWebRequest)WebRequest.Create(HyvesHttpUri);
            this.httpWebRequest.Method = "POST";

            // allows for validation of SSL conversations (Only for Dev)
            // ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

            this.requestCallback = requestCallback;
            this.parameters = parameters;

            this.RequestResult = requestResult;

            ValidateParameters(parameters);

            parameters["ha_method"] = EnumHelper.GetDescription(method);
            parameters["ha_version"] = ApiVersion;
            parameters["ha_format"] = "json";
            parameters["ha_fancylayout"] = useFancyLayout.ToString().ToLower();
            if (page > 0) parameters["ha_page"] = page.ToString();
            if (resultsPerPage > 0) parameters["ha_resultsperpage"] = resultsPerPage.ToString();

            StringBuilder requestBuilder = new StringBuilder(1024);

            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                if (requestBuilder.Length != 0)
                {
                    requestBuilder.Append("&");
                }

                requestBuilder.Append(parameter.Key);
                requestBuilder.Append("=");
                requestBuilder.Append(parameter.Value);
            }

            string timeStamp = oAuthBase.GenerateTimeStamp();
            
            string nonce = oAuthBase.GenerateNonce();
            if (string.IsNullOrEmpty(token) == false)
            {
                parameters["oauth_token"] = token;
            }

            parameters["oauth_consumer_key"] = ConsumerKey;
            parameters["oauth_timestamp"] = timeStamp;
            parameters["oauth_nonce"] = nonce;
            parameters["oauth_version"] = "1.0";
            parameters["oauth_signature_method"] = "HMAC-SHA1";

            parameters["oauth_signature"] = oAuthBase.GenerateSignature(new Uri(HyvesHttpUri), parameters, ConsumerKey, ConsumerSecret, token, tokenSecret, httpWebRequest.Method, timeStamp, nonce);
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";

            httpWebRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), this);
        }

        // Web request callback
        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            StringBuilder requestBuilder = new StringBuilder(1024);
            HyvesRequest<T> hyvesRequest = (HyvesRequest<T>)asynchronousResult.AsyncState;

            foreach (KeyValuePair<string, string> parameter in hyvesRequest.Parameters)
            {
                if (requestBuilder.Length != 0)
                {
                    requestBuilder.Append("&");
                }

                requestBuilder.Append(parameter.Key);
                requestBuilder.Append("=");
                requestBuilder.Append(oAuthBase.UrlEncode(parameter.Value));
            }

            byte[] requestBytes = Encoding.UTF8.GetBytes(requestBuilder.ToString());

            HttpWebRequest httpWebRequest = hyvesRequest.HttpWebRequest;

            // End the operation
            Stream postStream = httpWebRequest.EndGetRequestStream(asynchronousResult);

            // Write to the request stream.
            postStream.Write(requestBytes, 0, requestBytes.Length);
            postStream.Close();

            // Start the asynchronous operation to get the response
            httpWebRequest.BeginGetResponse(new AsyncCallback(GetResponseCallback), hyvesRequest);
        }

        private bool ValidateRemoteCertificate(object sender, X509Certificate certificate, X509Chain chain,SslPolicyErrors policyErrors)
        {
            return true;
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            HyvesRequest<T> hyvesRequest = (HyvesRequest<T>)asynchronousResult.AsyncState;
            HttpWebRequest httpWebRequest = hyvesRequest.HttpWebRequest;

            // End the operation
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)httpWebRequest.EndGetResponse(asynchronousResult);
            }
            catch (WebException e) 
            {
                WebResponse r = e.Response;
                Stream s = r.GetResponseStream();

                //Error occurred, calling callback
                this.RequestResult.Message = new StreamReader(s).ReadToEnd();
                this.RequestResult.IsError = true;
                this.RequestResult.Execption = e;
                hyvesRequest.RequestCallback(this.RequestResult);
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
            this.RequestResult.Response = responseString;
            this.RequestResult.IsError = false;
            hyvesRequest.RequestCallback(this.RequestResult);
        }

        private bool ValidateParameters(Dictionary<string, string> parameters)
        {
            //Todo: throw exception if one of the ha prameter is used the parameter
            return true;
        }
    }
}
