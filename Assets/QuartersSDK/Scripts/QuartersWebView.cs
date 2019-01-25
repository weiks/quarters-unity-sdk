using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace QuartersSDK {
    public class QuartersWebView : MonoBehaviour {
        
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OpenWebView(string url);

        [DllImport("__Internal")]
        private static extern void CloseWebView();
#endif

        public delegate void OnDeepLinkDelegate(string url, bool isExternalBrowser);
        public static OnDeepLinkDelegate OnDeepLink;

        public delegate void OnCancelledDelegate();
        public static OnCancelledDelegate OnCancelled;

        private static string LastOpenedUrl = "";


        public static QuartersWebView OpenURL(string url) {

            //UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;
            Debug.Log("Web view open url: " + url);
            
            
            GameObject webViewGO= new GameObject("QuartersWebView");
            QuartersWebView quartersWebView = webViewGO.AddComponent<QuartersWebView>();

            
#if UNITY_WEBGL && !UNITY_EDITOR
            //webGL plugin show webview
            LastOpenedUrl = url;
            OpenWebView(url);
#else
  
            UniWebView webView = webViewGO.AddComponent<UniWebView>();
            SetFrameSize(webView);

            webView.Load(url);
            webView.Show(false, UniWebViewTransitionEdge.Bottom);
            webView.OnPageStarted += quartersWebView.OnUrlOpenWebView;


            //handle autorotation
            webView.OnOreintationChanged += (view, orientation) => {
                SetFrameSize(webView);
            };

            webView.OnShouldClose += ShouldClose;
#endif

            return quartersWebView;
        }



        public void OnUrlOpenWebView(UniWebView webView, string url) {
            
            Debug.Log("OnUrlOpenWebView " + url);
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
            
            if (LastOpenedUrl == url) return;
            
            Debug.Log("OnUrlOpenWebGL " + url);
            if (url.IsDeepLink()) {
                //deep link opened
                if (OnDeepLink != null) OnDeepLink(url, isExternalBrowser: false);
                CloseWebView();
            }
            else {
                if (url != url) {
                    //external link, open external browser instead and invalidate this webview
                    Application.OpenURL(url);
                }
            }
            
        }

        
        public void OnWebViewClosed() {
            Debug.Log("OnWebViewClosed");
        }

        public void OnWebViewReceivedData(string data) {
            Debug.Log("OnWebViewReceivedData " + data);
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
            webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
        }



    }




}
