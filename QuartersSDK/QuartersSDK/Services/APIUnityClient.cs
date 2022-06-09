using NetTopologySuite.Utilities;
using Newtonsoft.Json;
using QuartersSDK.Data;
using QuartersSDK.Data.Enums;
using QuartersSDK.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace QuartersSDK.Services
{
    public class APIUnityClient : IAPIClient
    {
     
        private IEnumerator DoPost(string url, WWWForm data)
        {
            using (UnityWebRequest request = UnityWebRequest.Post(url, data))
            {
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError(request.error);
                    Debug.LogError(request.downloadHandler.text);

                    Error error = new Error(request.downloadHandler.text);

                    //if (error.ErrorDescription == Error.INVALID_TOKEN)
                    //{
                    //    //dispose invalid refresh token
                    //    refreshToken = "";
                    //}
                }
            }
        }

        public ResponseData RequestPost(string url, RequestData request)
        {
            ResponseData rdo = new ResponseData();
            Dictionary<string, string> responseData;
            WWWForm data = new WWWForm();
            data.AddField("grant_type", "refresh_token");
            data.AddField("client_id", request.ClientId);
            data.AddField("client_secret", request.ClientSecret);
            data.AddField("refresh_token", request.RefreshToken);
            if (!String.IsNullOrEmpty(request.CodeVerifier))
                data.AddField("code_verifier", request.CodeVerifier);
            if (!String.IsNullOrEmpty(request.Code))
                data.AddField("code", request.Code);

            var result = DoPost(url, data);

            responseData = JsonConvert.DeserializeObject<Dictionary<string, string>>(((UnityWebRequest)result).downloadHandler.text);
            return rdo;
        }

    }
}
