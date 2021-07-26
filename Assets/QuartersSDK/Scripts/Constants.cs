using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace QuartersSDK {
    public class Error {

        public static string UNAUTHORIZED_ERROR = "401 Unauthorized";
        public static string INVALID_TOKEN = "Invalid `refresh_token`";


        [JsonProperty("error")] public string ErrorMessage;
        [JsonProperty("error_description")] public string ErrorDescription;
        
        public Error() {}

        public Error(string json) {
            Error error = JsonConvert.DeserializeObject<Error>(json);
            this.ErrorMessage = error.ErrorMessage;
            this.ErrorDescription = error.ErrorDescription;
        }
        
    }

    public enum Environment {
        sandbox,
		development,
		production
	}


    public static partial class Constants {
        public const string REFRESH_TOKEN_KEY = "QuartersRefreshToken";
        public const string GUEST_TOKEN_KEY = "QuartersGuestToken";
        public const string GUEST_FIREBASE_TOKEN = "QuartersGuestFirebaseToken";

    }

    public enum Scope {
        identity,
        email,
        transactions,
        events,
        wallet
    }
    
}
