using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
#if UNITY_STANDALONE_OSX
    public class MacLinkProvider : ILinkProvider
    {
        private readonly bool _isSteamBuild;
#if ENABLE_IL2CPP
        [DllImport("MacIL2CPP")]
#else

        [DllImport("__Internal")]
#endif
        static extern void DeepLink_RegisterCallback(UniversalLinkCallback callback);

        [AOT.MonoPInvokeCallback(typeof(UniversalLinkCallback))]
        private static void OnCompletionCallback(string link)
        {
            _lastInstance.OnLinkReceived(link);
        }

        private bool _receivedDataFromLib = false;

        private static string _lastLink;

        private static MacLinkProvider _lastInstance;

        public MacLinkProvider(bool isSteamBuild)
        {
            _isSteamBuild = isSteamBuild;
            _lastInstance = this;
            Initialize();
        }

        public bool Initialize()
        {
            DeepLink_RegisterCallback(OnCompletionCallback);
            return true;
        }

        private event Action<string> _linkReceived;

        public event Action<string> LinkReceived
        {
            add
            {
                _linkReceived += value;

                if (_receivedDataFromLib == false)
                    return;

                OnLinkReceived(_lastLink);
                _lastLink = null;
                _receivedDataFromLib = false;

            }
            remove { _linkReceived -= value; }
        }

        public void PollInfoAfterPause()
        {

        }

        private void OnLinkReceived(string lastLink)
        {
            var handler = _linkReceived;

            if (handler != null)
            {
                //
                //  If is a valid link or is not but we are on steam and need to sync up data
                //
                if (string.IsNullOrEmpty(lastLink) == false ||
                    string.IsNullOrEmpty(lastLink) && _isSteamBuild)
                    handler(lastLink);

            }
            else
            {
                _receivedDataFromLib = true;
                _lastLink = lastLink;
            }
        }
    }
#endif
    }
