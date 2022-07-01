using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
#if UNITY_ANDROID
    public class AndroidLinkProvider : ILinkProvider
    {
        private AndroidJavaClass _unityPlayer = null;

        private AndroidJavaObject _currentActivity = null;

        private AndroidJavaObject _deepLinkIntent = null;

        private string _deepLink = null;

       
        public bool Initialize()
        {

            _unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");

            if (_unityPlayer == null)
                return false;

            _currentActivity = _unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (_currentActivity != null)
            {
                VerifyIfHasOpenByLink();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void VerifyIfHasOpenByLink()
        {
#if DEBUG_UDL
            Debug.Log("[UDL - AndroidLinkProvider] VerifyIfHasOpenByLink ");
#endif


            const string extraName = "____io_extra_udl";
            if (_currentActivity != null)
            {
                try
                {
#if DEBUG_UDL
                    Debug.Log("[UDL - AndroidLinkProvider] getIntent ");
#endif
                    _deepLinkIntent = _currentActivity.Call<AndroidJavaObject>("getIntent");

                    if (_deepLinkIntent != null)
                    {
#if DEBUG_UDL
                        Debug.Log("[UDL - AndroidLinkProvider] hasExtra ");
#endif
                        var hasExtra = _deepLinkIntent.Call<bool>("hasExtra", extraName);

                        var arguments = safeCallStringMethod(_deepLinkIntent, "getDataString");
#if DEBUG_UDL
                        Debug.Log("[UDL - AndroidLinkProvider] DataString --> " + arguments);
#endif

                        if (arguments != null && hasExtra == false && IsValidUri(arguments))
                        {
                            _deepLink = arguments;
#if DEBUG_UDL
                            Debug.Log("[UDL - AndroidLinkProvider] OnLinkReceived ");
#endif
                            OnLinkReceived(_deepLink);

                            _deepLinkIntent.Call<AndroidJavaObject>("putExtra", extraName, "dne");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("UDL: ERROR \n" + e);

                }
            }
        }

        public static string safeCallStringMethod(AndroidJavaObject javaObject, string methodName, params object[] args)
        {
            if (args == null) args = new object[] { null };
            IntPtr methodID = AndroidJNIHelper.GetMethodID<string>(javaObject.GetRawClass(), methodName, args, false);
            jvalue[] jniArgs = AndroidJNIHelper.CreateJNIArgArray(args);

            try
            {
                IntPtr returnValue = AndroidJNI.CallObjectMethod(javaObject.GetRawObject(), methodID, jniArgs);
                if (IntPtr.Zero != returnValue)
                {
                    var val = AndroidJNI.GetStringUTFChars(returnValue);
                    AndroidJNI.DeleteLocalRef(returnValue);
                    return val;
                }
            }
            finally
            {
                AndroidJNIHelper.DeleteJNIArgArray(args, jniArgs);
            }

            return null;

        }

        private event Action<string> _linkReceived;

        public event Action<string> LinkReceived
        {
            add
            {
                _linkReceived += value;

                if (string.IsNullOrEmpty(_deepLink))
                    return;

                OnLinkReceived(_deepLink);
                _deepLink = null;

            }
            remove { _linkReceived -= value; }
        }

        public void PollInfoAfterPause()
        {
            VerifyIfHasOpenByLink();
        }

        private void OnLinkReceived(string lastLink)
        {
            var handler = _linkReceived;
            if (handler != null)
                handler(lastLink);
        }

        private static bool IsValidUri(string url)
        {
            Uri uri;
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri) == false)
                return false;


            if (url.StartsWith("content://") || url.StartsWith("file://"))
                return false;

            try
            {
                if (File.Exists(url))
                    return false;
            }
            catch (Exception e)
            {
            }

            return true;
        }
    }
#endif


}
