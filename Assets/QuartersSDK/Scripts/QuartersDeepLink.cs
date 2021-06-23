using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace QuartersSDK {
    public class QuartersDeepLink : MonoBehaviour {
        
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


        private void Awake() {
            
            Application.deepLinkActivated += ApplicationOnDeepLinkActivated;
            if (!string.IsNullOrEmpty(Application.absoluteURL)) {
                ApplicationOnDeepLinkActivated(Application.absoluteURL);
            }
            
        }

        private void ApplicationOnDeepLinkActivated(string url) {
            Debug.Log($"ApplicationOnDeepLinkActivated: {url} is valid deep link: {url.IsValidDeepLink()}");
            
            if (url.IsValidDeepLink()) {
                
                //deep link opened
                if (OnDeepLink != null) OnDeepLink(url, isExternalBrowser: false);
                    
            }
            
            
        }


        public static void OpenURL(string url) {
            
            Debug.Log("Web view open url: " + url);
            
            Application.OpenURL(url);
        
            
//             GameObject webViewGO = new GameObject("QuartersWebView");
//             QuartersDeepLink quartersDeepLink = webViewGO.AddComponent<QuartersDeepLink>();
//
//             
// #if UNITY_WEBGL && !UNITY_EDITOR
//             //webGL plugin show webview
//             LastOpenedUrl = url;
//             OpenWebView(url);
// #else
//   
//             UniWebView webView = webViewGO.AddComponent<UniWebView>();
//             webView.CleanCache();
//             SetFrameSize(webView);
//
//             
//             webView.Load(url);
//             webView.Show(false, UniWebViewTransitionEdge.Bottom);
//             webView.OnPageStarted += quartersDeepLink.OnUrlOpenWebView;
//
//             //handle autorotation
//             webView.OnOrientationChanged += (view, orientation) => {
//                 SetFrameSize(webView);
//             };
//
//             webView.OnShouldClose += ShouldClose;
// #endif
//
//             return quartersDeepLink;
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
        
        





    }




}
