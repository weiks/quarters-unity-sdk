using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
    public static class Error {

        public static string UNAUTHORIZED_ERROR = "401 Unauthorized";

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
        events
    }
    
}
