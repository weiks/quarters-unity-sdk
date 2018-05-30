using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QuartersSDK {
	public static class URIParser {

	    public static Dictionary<string, string> ParseURI(this string escapedURL) {

	        Dictionary<string, string> result = new Dictionary<string, string>();

	        //unescape URL
	        string unescapedURL = WWW.UnEscapeURL(escapedURL);

	        if (unescapedURL.Contains("?")) {
	            string[] urlParamsSplit = unescapedURL.Split('?');

	            if (urlParamsSplit.Length > 1) {

	                if (unescapedURL.Contains("&")) {
	                    string[] parameters = urlParamsSplit[1].Split('&');

	                    foreach (string p in parameters) {

	                        string[] pSplit = p.Split('=');

	                        KeyValuePair<string, string> parameter = new KeyValuePair<string, string>(pSplit[0], pSplit[1]);
	                        result.Add(parameter.Key, parameter.Value);
	                    }

	                }
	            }
	        }

	        return result;
	       
	    }


        public static bool IsDeepLink(this string url) {

            bool result = false;
            if (url.Contains(Application.identifier.ToLower())) {
                result = url.StartsWith(Application.identifier.ToLower());
            }

            return result;
        }
	}
}


