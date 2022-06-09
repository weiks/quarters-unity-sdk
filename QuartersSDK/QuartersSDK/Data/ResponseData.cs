using Newtonsoft.Json;
using QuartersSDK.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace QuartersSDK.Data
{
    public class ResponseData : Serializable
    {
        private long _balance;

        public int StatusCode;
        public bool IsSuccesful = false;
        public Error ErrorResponse;

        [JsonProperty("refresh_token")]
        public string RefreshToken;

        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("scope")]
        public string Scope;

        [JsonProperty("balance")]
        public long Balance {
            get { return _balance; }
            set { _balance = long.Parse(value.ToString()); }
        }

        public ResponseData() { } 
        public ResponseData(Error err) { ErrorResponse = err; }
        public ResponseData(string json, HttpStatusCode status) 
        {
            this.SetData(json, status);
        }

        public void SetData(string json, HttpStatusCode status)
        {
            var aux = JsonConvert.DeserializeObject<ResponseData>(json);
            StatusCode = (int)status;
            Balance = aux.Balance;
            AccessToken = aux.AccessToken;
            RefreshToken= aux.RefreshToken;
            Scope = aux.Scope;
            IsSuccesful = true;
        }

        public void SetError(Error err, HttpStatusCode status)
        {
            StatusCode = (int)status;
            ErrorResponse = err;
        }

    }
}
