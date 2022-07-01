using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace QuartersSDK {
    [Serializable]
    public class User {
        [JsonProperty("id")] public string Id = "";
        [JsonProperty("gamerTag")] public string GamerTag = "";
        [JsonProperty("email")] public string Email = "";
        [JsonProperty("avatar")] public string AvatarUrl = "";


        private long balance = 0;

        public long Balance {
            set { balance = value; }

            get { return balance; }
        }

        public string AvailableCoinsString {
            get { return String.Format("{0:n0}", balance); }
        }
    }
}