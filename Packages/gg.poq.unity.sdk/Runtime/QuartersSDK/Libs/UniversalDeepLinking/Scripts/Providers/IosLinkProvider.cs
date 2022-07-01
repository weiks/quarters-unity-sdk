using System;
using System.Runtime.InteropServices;



namespace ImaginationOverflow.UniversalDeepLinking.Providers
{
#if UNITY_IOS || UNITY_TVOS
    public class IosLinkProvider : ILinkProvider
    {

        [DllImport("__Internal")]
        static extern void DeepLink_RegisterCallback(UniversalLinkCallback callback);

        [AOT.MonoPInvokeCallback(typeof(UniversalLinkCallback))]
        private static void OnCompletionCallback(string link)
        {
            _lastInstance.OnLinkReceived(link);
        }

        private static string _lastLink;

        private static IosLinkProvider _lastInstance;

        public IosLinkProvider()
        {
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

                if (string.IsNullOrEmpty(_lastLink))
                    return;

                OnLinkReceived(_lastLink);
                _lastLink = null;

            }
            remove { _linkReceived -= value; }
        }

        public void PollInfoAfterPause()
        {

        }

        private void OnLinkReceived(string lastLink)
        {
            var handler = _linkReceived;
            if (handler == null)
            {
                _lastLink = lastLink;
            }
            else
                handler(lastLink);
        }

    }
#endif
}
