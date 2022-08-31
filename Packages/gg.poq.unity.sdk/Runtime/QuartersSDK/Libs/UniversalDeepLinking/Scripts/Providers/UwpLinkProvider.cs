using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
#if UNITY_WSA
    public class UwpLinkProvider : ILinkProvider
    {
        private static string _lastLink;
        private static char _lastMark = '-';
        private static UwpLinkProvider _lastInstance;
        /// <summary>
        /// Used to diferentiate between using URI and the ProcessLink method
        /// </summary>
        private static bool _directFrameworkSet;

        public UwpLinkProvider()
        {
            _lastInstance = this;
        }

        public bool Initialize()
        {
            return true;
        }

        private event Action<string> _linkReceived;

        public event Action<string> LinkReceived
        {
            add
            {
                _linkReceived += value;

                CheckForData();
            }
            remove { _linkReceived -= value; }
        }

        private void CheckForData()
        {
            if (string.IsNullOrEmpty(_lastLink) && (_directFrameworkSet || TryProcessUriArgument() == false))
                return;

            OnLinkReceived(_lastLink);
            _lastLink = null;
        }

        public void PollInfoAfterPause()
        {
            CheckForData();
        }

        private bool TryProcessUriArgument()
        {
            var args = UnityEngine.WSA.Application.arguments;

            if (string.IsNullOrEmpty(args) || args.Contains("Uri=") == false)
                return false;

            _lastLink = args.Replace("Uri=", string.Empty);

            if (string.IsNullOrEmpty(_lastLink))
                return false;

            var currMark = _lastLink[0];


            if (currMark == _lastMark)
            {
                _lastLink = null;
                return false;
            }

            _lastMark = currMark;

            _lastLink = _lastLink.Substring(1);

            return !string.IsNullOrEmpty(_lastLink);

        }


        public static void ProcessLink(string url)
        {
            _directFrameworkSet = true;
            if (_lastInstance == null)
                _lastLink = url;
            else
                _lastInstance.OnLinkReceived(url);
        }

        protected virtual void OnLinkReceived(string obj)
        {
            if (UnityEngine.WSA.Application.RunningOnAppThread())
            {
                var handler = _linkReceived;
                if (handler != null) handler(obj);
            }
            else
                UnityEngine.WSA.Application.InvokeOnAppThread(() => OnLinkReceived(obj), false);

        }
    }

#endif
}
