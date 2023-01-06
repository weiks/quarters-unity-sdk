﻿using Newtonsoft.Json;
using System;
using System.Net;

namespace QuartersSDK.Data
{
    public class Error : Exception
    {
        public static string INVALID_TOKEN = "Invalid `refresh_token`";
        public static string INVALID_CREDENTIALS = "Unknown client credentials";
        

        [JsonProperty("error")]
        public string ErrorMessage;

        [JsonProperty("error_description")]
        public string ErrorDescription;

        [JsonProperty("status_code")]
        public HttpStatusCode StatusCode;

        public Error()
        {
        }

        public Error(string json)
        {
            try
            {
                var err = JsonConvert.DeserializeObject<Error>(json);
                this.ErrorMessage = err.ErrorMessage ?? json;
                this.ErrorDescription = err.ErrorDescription ?? String.Empty;
                this.StatusCode = string.IsNullOrEmpty(err.StatusCode.ToString()) ? err.StatusCode : HttpStatusCode.BadRequest;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Error(string message, string description)
        {
            this.ErrorMessage = message;
            this.ErrorDescription = description;
        }
    }
}