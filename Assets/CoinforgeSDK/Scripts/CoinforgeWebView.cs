using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace CoinforgeSDK {
    public class CoinforgeWebView : MonoBehaviour {
        
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OpenWebView(string url);

        [DllImport("__Internal")]
        private static extern void CloseWebView();
#endif

        public delegate void OnDeepLinkDelegate(string url, bool isExternalBrowser);
        public static OnDeepLinkDelegate OnDeepLink;
        
        public delegate void OnDeepLinkWebGLDelegate(Dictionary<string, string> webViewData);
        public static OnDeepLinkWebGLDelegate OnDeepLinkWebGL;

        public delegate void OnCancelledDelegate();
        public static OnCancelledDelegate OnCancelled;

        private static string LastOpenedUrl = "";


        public static CoinforgeWebView OpenURL(string url) {
            
            Debug.Log("Web view open url: " + url);
            
            
            GameObject webViewGO= new GameObject("CoinforgeWebView");
            CoinforgeWebView coinforgeWebView = webViewGO.AddComponent<CoinforgeWebView>();

            
#if UNITY_WEBGL && !UNITY_EDITOR
            //webGL plugin show webview
            LastOpenedUrl = url;
            OpenWebView(url);
#else
  
            UniWebView webView = webViewGO.AddComponent<UniWebView>();
            webView.CleanCache();
            SetFrameSize(webView);

            
            webView.Load(url);
            webView.Show(false, UniWebViewTransitionEdge.Bottom);
            webView.OnPageStarted += coinforgeWebView.OnUrlOpenWebView;

            //handle autorotation
            webView.OnOrientationChanged += (view, orientation) => {
                SetFrameSize(webView);
            };

            webView.OnShouldClose += ShouldClose;
#endif

            return coinforgeWebView;
        }



        public void OnUrlOpenWebView(UniWebView webView, string url) {
            
            Debug.Log("OnUrlOpenWebView " + url + " is deep link: " + url.IsDeepLink());
            if (url.IsDeepLink()) {
                //deep link opened
                if (OnDeepLink != null) OnDeepLink(url, isExternalBrowser: false);
                webView.Hide(false);
            }
            else {
                if (url != url) {
                    //external link, open external browser instead and invalidate this webview
                    webView.Stop();
                    webView.Hide();
                    Application.OpenURL(url);
                }
            }
            
        }

        
        
        
        #region WebGL plugin events
        
#if UNITY_WEBGL
        public void OnUrlOpen(string url) {
            
//            if (LastOpenedUrl == url) return;
//            
//            Debug.Log("On Url Open Web GL is deep link: " + url.IsDeepLink() + " " + url);
//            
//            if (url.IsDeepLink()) {
//                //deep link opened
//                if (OnDeepLink != null) OnDeepLink(url, isExternalBrowser: false);
//                CloseWebView();
//            }
//            else {
//                if (url != url) {
//                    //external link, open external browser instead and invalidate this webview
//                    Application.OpenURL(url);
//                }
//            }
            
        }

        
        public void OnWebViewClosed() {
            Debug.Log("OnWebViewClosed");
            if (OnCancelled != null) OnCancelled();
        }

        public void OnWebViewReceivedData(string data) {
            Debug.Log("OnWebViewReceivedData " + data);

            Dictionary<string, string> webViewData = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            if (OnDeepLinkWebGL != null) OnDeepLinkWebGL(webViewData);
        }

#endif
#endregion
        
        


        //Should close is handling other than browser controlled actions, like back button on Android
        public static bool ShouldClose(UniWebView view) {

            Debug.Log("Should close");

            view.Hide();
            if (OnCancelled != null) OnCancelled();

            return true;
        }


        private static void SetFrameSize(UniWebView webView) {

            if (Application.isEditor) {
                webView.Frame = new Rect(0, 0, 500f, 500f);
            }
            else {
                webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            }
            
            
        }



    }




}
