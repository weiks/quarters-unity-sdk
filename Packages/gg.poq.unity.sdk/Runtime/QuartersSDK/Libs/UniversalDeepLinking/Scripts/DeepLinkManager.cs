using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using ImaginationOverflow.UniversalDeepLinking.Providers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ImaginationOverflow.UniversalDeepLinking
{

    /// <summary>
    /// Manages all deep linking and Domain association activations
    /// </summary>
    public sealed class DeepLinkManager
    {
        /// <summary>
        /// DeepLinkManager static instance
        /// </summary>
        public static DeepLinkManager Instance { get; private set; }

        /// <summary>
        /// Gets or sets if the current build is running on Steam, more info on the documentation: https://universaldeeplinking.imaginationoverflow.com/docs/
        /// </summary>
        public bool IsSteamBuild { get; set; }

        static DeepLinkManager()
        {
            Instance = new DeepLinkManager();
        }

        private DeepLinkManager() { }

        /// <summary>
        /// Adds/Removes an event handler that will be called when a new Deep Link or Domain activation occurs.
        /// </summary>
        public event LinkActivationHandler LinkActivated
        {
            add
            {
#if DEBUG_UDL
                Debug.Log("[UDL - DeepLinkManager] Added Activation Handler");
#endif
                _activated += value;

                RegisterIfNecessary();
            }
            remove
            {
#if DEBUG_UDL
                Debug.Log("[UDL - DeepLinkManager] Removed Activation Handler");
#endif
                _activated -= value;
            }
        }


        private event LinkActivationHandler _activated;

        private ILinkProvider _currProvider;
        private GameObject _go;
        private string _storedActivation;

        /// <summary>
        /// Creates a LinkActivation instance and calls all LinkActivated handlers.
        /// </summary>
        /// <param name="args"></param>
        public void ManuallyTriggerDeepLink(string args)
        {
            _currProvider_LinkReceived(args);
        }

        private void RegisterIfNecessary()
        {
            if (_currProvider != null)
            {
                if (string.IsNullOrEmpty(_storedActivation) == false)
                {
                    var activation = _storedActivation;
                    _storedActivation = null;
                    _currProvider_LinkReceived(activation);
                }
                return;
            }
            switch (Application.platform)
            {
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    _currProvider = new EditorLinkProvider();
                    break;
#if !UDL_DLL_BUILD //Used to generate the other Unity Packageva

#if UNITY_ANDROID
                case RuntimePlatform.Android:
                    _currProvider = new AndroidLinkProvider();
                    break;
#endif
#if UNITY_IOS || UNITY_TVOS
                case RuntimePlatform.IPhonePlayer:
                    _currProvider = new IosLinkProvider();
                    break;
#endif
#if UNITY_WSA
                case RuntimePlatform.WSAPlayerARM:
                case RuntimePlatform.WSAPlayerX64:
                case RuntimePlatform.WSAPlayerX86:
                    _currProvider = new UwpLinkProvider();
                    break;
#endif
#if UNITY_STANDALONE_LINUX
                case RuntimePlatform.LinuxPlayer:
                    _currProvider = new LinuxLinkProvider(IsSteamBuild);
                    break;
#endif
#if UNITY_STANDALONE_OSX
                case RuntimePlatform.OSXPlayer:
                    _currProvider = new MacLinkProvider(IsSteamBuild);
                    break;
#endif
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
                case RuntimePlatform.WindowsPlayer:
                    _currProvider = new WindowsLinkProvider(IsSteamBuild);
                    break;
#endif

#endif
                default:
                    _currProvider = new LinkProviderFactory().GetProvider(IsSteamBuild);
                    break;
            }



            CreatePauseGameObject();

            try
            {
                if (_currProvider.Initialize() == false)
                {
                    throw new Exception("UDL:Init error");
                }

                _currProvider.LinkReceived += _currProvider_LinkReceived;
            }
            catch (Exception e)
            {

                Debug.LogError("UDL: Error Initializing Provider " + _currProvider.GetType().Name);
                _currProvider = null;
                Debug.LogError(e);
            }

            CreatePauseGameObject();
        }

        private void CreatePauseGameObject()
        {
            if (_go != null)
            {
                if (_go.GetComponent<UniversalDeeplinkingRuntimeScript>() != null)
                    return;

                Object.Destroy(_go);
                _go = null;
            }

            try
            {

                _go = new GameObject { name = "UniversalDeeplinking" };
                _go.AddComponent<UniversalDeeplinkingRuntimeScript>();

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void _currProvider_LinkReceived(string s)
        {
            if (OnActivated(s))
                return;

            StoreActivation(s);
        }

        private void StoreActivation(string s)
        {
#if DEBUG_UDL
            Debug.Log("[UDL - DeepLinkManager] Storing activation for future callbacks");
#endif
            _storedActivation = s;
        }


        private bool OnActivated(string s)
        {

#if DEBUG_UDL
            Debug.Log("[UDL - DeepLinkManager] Calling callbacks");
#endif
            LinkActivation la = CreateLinkActivation(s);
            var handler = _activated;
            if (handler != null)
            {
                try
                {
                    handler(la);
                }
                catch (Exception e)
                {

                    Debug.LogError("*ERROR* - DeepLinkManager LinkActivated handler throw an exception");
                    Debug.LogException(e);

                    if (Debug.isDebugBuild)
                        throw;

                }
                return true;
            }

#if DEBUG_UDL
            Debug.Log("[UDL - DeepLinkManager] No callbacks found");
#endif

            return false;
        }

        private LinkActivation CreateLinkActivation(string s)
        {


            var query = string.Empty;
            var args = new Dictionary<string, string>();
            try
            {
                var parser = new UrlEncodingParser(s);
                args = parser;
                query = parser.Query;
            }
            catch (Exception)
            {
            }

            return new LinkActivation(s, query, args);

        }


        internal void GameCameFromPause()
        {
#if DEBUG_UDL
            Debug.Log("[UDL - DeepLinkManager] GameCameFromPause " + (_currProvider != null));
#endif
            if (_currProvider != null)
                _currProvider.PollInfoAfterPause();
        }

        private class UrlEncodingParser : Dictionary<string, string>
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
    }
}
