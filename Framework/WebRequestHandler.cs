using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;


namespace PLib.Framework.Networking
{
    public class WebRequest
    {
        public UnityWebRequest Request;
        public UnityAction<WebRequest,int> ResponseHandler;

#if UNITY_EDITOR
        public int _debugReponseCodeToReturn;
        public WebRequest(int debugReponseCodeToReturn, string url, string method = "GET", UnityAction<WebRequest,int> responseHandler = null)
        {
            Request = new UnityWebRequest(url, method);
            Request.SendWebRequest();
            ResponseHandler = responseHandler;
            _debugReponseCodeToReturn = debugReponseCodeToReturn;
        }
#endif


        public WebRequest(string url, string method, UnityAction<WebRequest,int> responseHandler)
        {
            Request = new UnityWebRequest(url, method);
            ResponseHandler = responseHandler;
        }

        public bool InvokeCBIfDone()
        {

            bool toReturn = Request.isDone || Request.isNetworkError;
            if (toReturn)
#if UNITY_EDITOR
                ResponseHandler?.Invoke(this,_debugReponseCodeToReturn);
#else
                ResponseHandler?.Invoke(this,Request.responseCode);
#endif

            return toReturn;
        }
    }

    public class WebRequestHandler : MonoBehaviour
    {
        private List<WebRequest> _activeRequests = new List<WebRequest>();
        private List<WebRequest> _toRemove = new List<WebRequest>();

        public static Uri CombineUri(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri);
        }

        public static string CombineUriToString(string baseUri, string relativeOrAbsoluteUri)
        {
            return new Uri(new Uri(baseUri), relativeOrAbsoluteUri).ToString();
        }

        public void CreateWebRequest(string url,string method = "GET", UnityAction<WebRequest,int> responseHandler = null)
        {
            WebRequest toAdd = new WebRequest(url,method,responseHandler);
            _activeRequests.Add(toAdd);
        }

#if UNITY_EDITOR
        public void CreateWebRequest(int responseCodeToReturn, string url, string method = "GET", UnityAction<WebRequest,int> responseHandler = null)
        {
            WebRequest toAdd = new WebRequest(responseCodeToReturn,url, method, responseHandler);
            _activeRequests.Add(toAdd);
        }
#endif

        public void Update()
        {
            if (_activeRequests.Count == 0)
                return;

            for (int i = 0; i < _activeRequests.Count; ++i)
            {
                WebRequest toConsider = _activeRequests[i];
                if (toConsider.InvokeCBIfDone())
                    _toRemove.Add(toConsider);
            }

            if (_toRemove.Count > 0)
            {
                for (int i = 0; i < _toRemove.Count; ++i)
                {
                    _activeRequests.Remove(_toRemove[i]);
                }
                _toRemove.Clear();
            }
        }


    }
}