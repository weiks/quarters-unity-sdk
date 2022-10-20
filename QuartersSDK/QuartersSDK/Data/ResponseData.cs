﻿using Newtonsoft.Json;
using QuartersSDK.Data.Interfaces;
using System;
using System.Net;

namespace QuartersSDK.Data
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ResponseData :  ISerializable
    {
        private long _balance;

        public bool IsSuccesful = false;
        public Error ErrorResponse;

        [JsonProperty("id")]
        public string IdTransaction;

        [JsonProperty("refresh_token")]
        public string RefreshToken;

        [JsonProperty("access_token")]
        public string AccessToken;

        [JsonProperty("scope")]
        public string Scope;

        [JsonProperty("balance")]
        public long Balance
        {
            get { return _balance; }
            set { _balance = long.Parse(value.ToString()); }
        }

        public ResponseData()
        {
        }

        public ResponseData(Error err)
        {
            IsSuccesful = false;
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
                Balance = !string.IsNullOrEmpty(aux.Balance.ToString()) ? aux.Balance : (long)0;
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
            err.StatusCode = status;
            ErrorResponse = err;
            IsSuccesful = false;
        }

        public string ToJSONString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}