using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
    public static class Error {

        public static string Unauthorized = "401 Unauthorized";

    }

	public enum Environment {
		development,
		production
	}


    public static partial class Constants {
        public const string REFRESH_TOKEN_KEY = "QuartersRefreshToken";
        public const string GUEST_TOKEN_KEY = "QuartersGuestToken";
        public const string GUEST_FIREBASE_TOKEN = "QuartersGuestFirebaseToken";
    }
}
