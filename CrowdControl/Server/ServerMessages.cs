using System.Text;
using Newtonsoft.JsonCC;
using System.Net;
using System.IO;

using System;
using System.Collections.Generic;

namespace WarpWorld.CrowdControl {
    public static class ServerMessages {
        
        private static string OpenApiURL {
            get {
                switch (CrowdControl.CCServer) {
                    case Server.Dev:
                        return "https://dev-openapi.crowdcontrol.live/";
                    case Server.Production:
                        return "https://openapi.crowdcontrol.live/";
                    case Server.Staging:
                        return "https://staging-openapi.crowdcontrol.live/";
                }

                return string.Empty;
            }
        }

        public static void SendPost(string postType, Action<string> callback, object json = null, bool gameSession = true) {
            string url = gameSession ? string.Format("{0}game-session/{1}", OpenApiURL, postType) : string.Format("{0}{1}", OpenApiURL, postType);
            string jsonString = json != null ? JsonConvert.SerializeObject(json) : string.Empty;

            WebRequest request = WebRequest.Create(url);
            request.Method = "POST";
            request.Headers.Add("Authorization", "cc-auth-token " + CrowdControl.instance.CurrentUserHash);

            CrowdControl.Log(url);

            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);

            request.ContentType = "application/json";
            request.ContentLength = jsonBytes.Length;

            using (Stream requestStream = request.GetRequestStream()) {
                requestStream.Write(jsonBytes, 0, jsonBytes.Length);
            }

            CrowdControl.Log("SENT: " + jsonString);

            GetResponseAsync(request, callback, postType);
        }

        private static void GetResponseAsync(WebRequest request, Action<string> callback, string postType) {
            request.BeginGetResponse(new AsyncCallback((IAsyncResult asyncResult) => {
                try {
                    HttpWebRequest httpRequest = (HttpWebRequest)asyncResult.AsyncState;
                    using (WebResponse response = httpRequest.EndGetResponse(asyncResult)) {
                        if (((HttpWebResponse)response).StatusCode == HttpStatusCode.OK) {
                            using (Stream responseStream = response.GetResponseStream()) {
                                StreamReader reader = new StreamReader(responseStream);
                                CrowdControl.PostGetResponses.Enqueue(new KeyValuePair<Action<string>, string>(callback, reader.ReadToEnd()));
                            }
                        }
                        else {
                            CrowdControl.LogError("Request failed with status code: " + ((HttpWebResponse)response).StatusCode);
                        }
                    }
                }
                catch (WebException ex) {
                    HttpWebResponse errorResponse = ex.Response as HttpWebResponse;
                    if (errorResponse != null) {
                        CrowdControl.LogError("Request failed with status code: " + errorResponse.StatusCode);
                    }
                }
                catch (Exception ex) {
                    CrowdControl.LogError("An error occurred: " + ex.Message);
                }
            }), request);
        }

        public static void RequestGet(string getType, Action<string> callback) {
            string url = string.Format("{0}/{1}", OpenApiURL, getType);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers.Add("Authorization", "cc-auth-token " + CrowdControl.instance.CurrentUserHash);

            request.BeginGetResponse(new AsyncCallback((IAsyncResult asynchronousResult) => {
                HttpWebRequest responseRequest = (HttpWebRequest)asynchronousResult.AsyncState;

                try {
                    using (HttpWebResponse callbackResponse = (HttpWebResponse)responseRequest.EndGetResponse(asynchronousResult)) {
                        if (callbackResponse.StatusCode == HttpStatusCode.OK) {
                            using (Stream responseStream = callbackResponse.GetResponseStream()) {
                                StreamReader reader = new StreamReader(responseStream);
                                CrowdControl.PostGetResponses.Enqueue(new KeyValuePair<Action<string>, string>(callback, reader.ReadToEnd()));
                            }
                        }
                        else {
                            CrowdControl.LogError("Request failed with status code: " + callbackResponse.StatusCode);
                        }
                    }
                }
                catch (WebException ex) {
                    if (ex.Response != null) {
                        using (Stream errorResponseStream = ex.Response.GetResponseStream()) {
                            StreamReader errorReader = new StreamReader(errorResponseStream);
                            string errorResponseText = errorReader.ReadToEnd();
                            CrowdControl.Log("Error response: " + errorResponseText);
                        }
                    }
                    else {
                        CrowdControl.Log("Error: " + ex.Message);
                    }
                }
            }
            ), request);
        }
    }
}
