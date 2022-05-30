using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using ImaginationOverflow.UniversalDeepLinking;
using Newtonsoft.Json;
using UnityEditor;

namespace QuartersSDK {
    public class QuartersWebView : MonoBehaviour {

        public delegate void OnDeepLinkDelegate(QuartersLink link);
        public static OnDeepLinkDelegate OnDeepLink;

        public delegate void OnCancelledDelegate();
        public static OnCancelledDelegate OnCancelled;
        
        
        private UniWebView WebView;

        public void Init() {
            DeepLinkManager.Instance.LinkActivated += OnLinkActivated;
            
            
            WebView = this.gameObject.AddComponent<UniWebView>();
            WebView.SetShowToolbar(false);

            
            if (Application.isEditor) {
                float editorScaleFactor = 0.5f;
                WebView.Frame = new Rect(0, 0, Screen.width * editorScaleFactor, Screen.height * editorScaleFactor);
            }
            else {
                //full screen
                WebView.Frame = new Rect(0, 0, Screen.width, Screen.height);
            }

            WebView.OnPageStarted += WebViewOnOnPageStarted;
        }


        
        
        private void OnDestroy() {
            DeepLinkManager.Instance.LinkActivated -= OnLinkActivated;
            if (WebView != null) {
                WebView.OnPageStarted -= WebViewOnOnPageStarted;
            }
        }
        

        //webview
        private void WebViewOnOnPageStarted(UniWebView webview, string url) {
            Debug.Log($"WebViewOnOnPageStarted: {url}");
            QuartersLink link = QuartersLink.Create(url);

            if (link.Uri.IsValidDeepLink()) {
                //deep link opened
                WebView.Hide();
                if (OnDeepLink != null) OnDeepLink(link);
            }
        }

        

        
        
        //external universal link
        private void OnLinkActivated(LinkActivation linkActivation) {
            Debug.Log($"ApplicationOnDeepLinkActivated: {linkActivation.Uri} is valid deep link: {linkActivation.Uri.IsValidDeepLink()}");
            QuartersLink link = QuartersLink.Create(linkActivation.Uri);
            
            if (link.Uri.IsValidDeepLink()) {
                //deep link opened
                if (OnDeepLink != null) OnDeepLink(link);
                    
            }
        }

    

        public void OpenURL(string url, LinkType linkType) {

            if (Application.isEditor) {
                linkType = LinkType.WebView;
            }
            
            if (linkType == LinkType.WebView) {
                WebView.Load(url);
                WebView.Show();
            }
            else {
                //external authorisation
                Application.OpenURL(url);
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
        
        





    }




}
