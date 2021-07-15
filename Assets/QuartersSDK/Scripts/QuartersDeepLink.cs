using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using ImaginationOverflow.UniversalDeepLinking;
using Newtonsoft.Json;

namespace QuartersSDK {
    public class QuartersDeepLink : MonoBehaviour {
        
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void OpenWebView(string url);

        [DllImport("__Internal")]
        private static extern void CloseWebView();
#endif

        public delegate void OnDeepLinkDelegate(LinkActivation linkActivation);
        public static OnDeepLinkDelegate OnDeepLink;
        
        public delegate void OnDeepLinkWebGLDelegate(Dictionary<string, string> webViewData);
        public static OnDeepLinkWebGLDelegate OnDeepLinkWebGL;

        public delegate void OnCancelledDelegate();
        public static OnCancelledDelegate OnCancelled;




        private void Awake() {
            DeepLinkManager.Instance.LinkActivated += OnLinkActivated;
        }
        
        
        private void OnDestroy() {
            DeepLinkManager.Instance.LinkActivated -= OnLinkActivated;
        }

        private void OnLinkActivated(LinkActivation linkActivation) {
            Debug.Log($"ApplicationOnDeepLinkActivated: {linkActivation.Uri} is valid deep link: {linkActivation.Uri.IsValidDeepLink()}");
            
            if (linkActivation.Uri.IsValidDeepLink()) {
                
                //deep link opened
                if (OnDeepLink != null) OnDeepLink(linkActivation);
                    
            }
        }

    

        public static void OpenURL(string url) {
            
            Debug.Log("Web view open url: " + url);
            Application.OpenURL(url);
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
