using Newtonsoft.Json;
using QuartersSDK.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace QuartersSDK.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResponseData : HttpWebResponse, ISerializable
    {
        private long _balance;

        public bool IsSuccesful = false;
        public Error ErrorResponse;

        [JsonProperty("Id")]
        public string IdTransaction;

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
        public ResponseData(Error err) {
            SetError(err, HttpStatusCode.BadRequest);
        }
        public ResponseData(string json, HttpStatusCode status) 
        {
            this.SetData(json, status);
        }

        public void SetData(string json, HttpStatusCode status)
        {
            try
            {
                var aux = JsonConvert.DeserializeObject<ResponseData>(json);
                Balance = !string.IsNullOrEmpty(aux.Balance.ToString())? aux.Balance : (long)0;
                AccessToken = aux.AccessToken ?? string.Empty;
                RefreshToken = aux.RefreshToken ?? string.Empty;
                Scope = aux.Scope ?? string.Empty;
                IdTransaction = aux.IdTransaction;
                IsSuccesful = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetError(Error err, HttpStatusCode status)
        {
            ErrorResponse = err;
            IsSuccesful = false;
        }

        public string ToJSONString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
