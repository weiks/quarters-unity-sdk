using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace QuartersSDK {

    public class QuartersLink {

        public string Uri { get; private set; }

        /// <summary>
        /// The Uri query string.
        /// </summary>
        public string RawQueryString { get; private set; }

        /// <summary>
        /// Query string divided by the query parameters
        /// </summary>
        public Dictionary<string, string> QueryString { get; private set; }

        public QuartersLink(string uri, string rawQueryString, Dictionary<string,string> queryStringParams)
        {
            Uri = uri;
            RawQueryString = rawQueryString;
            QueryString = queryStringParams;
        }

        public override string ToString()
        {
            return Uri;
        }


        public static QuartersLink Create(string rawUri) {
            
            var query = string.Empty;
            var args = new Dictionary<string, string>();
            try
            {
                var parser = new UrlEncodingParser(rawUri);
                args = parser;
                query = parser.Query;
            }
            catch (Exception)
            {
                
            }

            return new QuartersLink(rawUri, query, args);
            
        }
        
 
     
        
      
        
    }
    
    
    
    public class UrlEncodingParser : Dictionary<string, string>
        {

            /// <summary>
            /// Holds the original Url that was assigned if any
            /// Url must contain // to be considered a url
            /// </summary>
            private string Url { get; set; }
            public string Query { get; private set; }
            /// <summary>
            /// Always pass in a UrlEncoded data or a URL to parse from
            /// unless you are creating a new one from scratch.
            /// </summary>
            /// <param name="queryStringOrUrl">
            /// Pass a query string or raw Form data, or a full URL.
            /// If a URL is parsed the part prior to the ? is stripped
            /// but saved. Then when you write the original URL is 
            /// re-written with the new query string.
            /// </param>
            public UrlEncodingParser(string queryStringOrUrl = null)
            {
                Url = string.Empty;

                if (!string.IsNullOrEmpty(queryStringOrUrl))
                {
                    Parse(queryStringOrUrl);
                }
            }


            /// <summary>
            /// Assigns multiple values to the same key
            /// </summary>
            /// <param name="key"></param>
            /// <param name="values"></param>
            public void SetValues(string key, IEnumerable<string> values)
            {
                foreach (var val in values)
                    Add(key, val);
            }

            /// <summary>
            /// Parses the query string into the internal dictionary
            /// and optionally also returns this dictionary
            /// </summary>
            /// <param name="query">
            /// Query string key value pairs or a full URL. If URL is
            /// passed the URL is re-written in Write operation
            /// </param>
            /// <returns></returns>
            public Dictionary<string, string> Parse(string query)
            {
                if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
                    Url = query;

                if (string.IsNullOrEmpty(query))
                    Clear();
                else
                {
                    int index = query.IndexOf('?');
                    if (index > -1)
                    {
                        if (query.Length >= index + 1)
                            query = query.Substring(index + 1);
                    }
                    Query = query;
                    var pairs = query.Split('&');
                    foreach (var pair in pairs)
                    {
                        int index2 = pair.IndexOf('=');
                        if (index2 > 0)
                        {
                            var key = pair.Substring(0, index2);
                            var value = pair.Substring(index2 + 1);

                            var origKey = key;
                            int val = 2;
                            while (ContainsKey(key))
                            {
                                key = origKey + val++;
                            }

                            Add(key, value);
                        }
                    }
                }

                return this;
            }

            /// <summary>
            /// Writes out the urlencoded data/query string or full URL based 
            /// on the internally set values.
            /// </summary>
            /// <returns>urlencoded data or url</returns>
            public override string ToString()
            {
                string query = string.Empty;
                foreach (string key in Keys)
                {
                    query += key + "=" + Uri.EscapeUriString(this[key]) + "&";
                }
                query = query.Trim('&');

                if (!string.IsNullOrEmpty(Url))
                {
                    if (Url.Contains("?"))
                        query = Url.Substring(0, Url.IndexOf('?') + 1) + query;
                    else
                        query = Url + "?" + query;
                }

                return query;
            }
        }

        
    
    
    
	public static class URIParser {


        public static bool IsValidDeepLink(this string url) {

            bool result = false;
            
            if (url.Contains(Quarters.URL_SCHEME)) {
                result = url.StartsWith(Quarters.URL_SCHEME);
            }
            
            return result;
        }
        
     
	}
}


