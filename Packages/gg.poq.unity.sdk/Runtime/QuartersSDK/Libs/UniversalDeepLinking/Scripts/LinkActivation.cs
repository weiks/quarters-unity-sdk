using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace ImaginationOverflow.UniversalDeepLinking
{
    public class LinkActivation
    {
        /// <summary>
        /// The Uri that caused the game to be activated
        /// </summary>
        public string Uri { get; private set; }

        /// <summary>
        /// The Uri query string.
        /// </summary>
        public string RawQueryString { get; private set; }

        /// <summary>
        /// Query string divided by the query parameters
        /// </summary>
        public Dictionary<string, string> QueryString { get; private set; }

        public LinkActivation(string uri, string rawQueryString, Dictionary<string,string> queryStringParams)
        {
            Uri = uri;
            RawQueryString = rawQueryString;
            QueryString = queryStringParams;
        }

        public override string ToString()
        {
            return Uri;
        }
    }
}
