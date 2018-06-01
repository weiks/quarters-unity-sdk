using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
    public class QuartersWebView : MonoBehaviour {

        public delegate void OnDeepLinkDelegate(string url, bool isExternalBrowser);
        public static OnDeepLinkDelegate OnDeepLink;

        public delegate void OnCancelledDelegate();
        public static OnCancelledDelegate OnCancelled;


        public static QuartersWebView OpenURL(string url) {

            //UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;

            Debug.Log("Web view open url: " + url);
            
            GameObject webViewGO= new GameObject("QuartersWebView");
            QuartersWebView quartersWebView = webViewGO.AddComponent<QuartersWebView>();

            UniWebView webView = webViewGO.AddComponent<UniWebView>();
            SetFrameSize(webView);

            webView.Load(url);
            webView.Show(false, UniWebViewTransitionEdge.Bottom);


            webView.OnPageStarted += (UniWebView view, string u) => {

                Debug.Log("OnPageStarted " + u);
                if (u.IsDeepLink()) {
                    //deep link opened
                    if (OnDeepLink != null) OnDeepLink(u, isExternalBrowser: false);
                    webView.Hide(false);
                }
                else {
                    if (u != url) {
                        //external link, open external browser instead and invalidate this webview
                        webView.Stop();
                        webView.Hide();
                        Application.OpenURL(u);
                    }
                }
            };


            //handle autorotation
            webView.OnOreintationChanged += (view, orientation) => {
                SetFrameSize(webView);
            };

            webView.OnShouldClose += ShouldClose;


            return quartersWebView;
        }



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
