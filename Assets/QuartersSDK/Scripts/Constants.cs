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


    public static class Constants {
        public const string REFRESH_TOKEN_KEY = "QuartersRefreshToken";
    }
}
