using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
    public class QuartersWebView : MonoBehaviour {

        public delegate void OnDeepLinkDelegate(string url);
        public static OnDeepLinkDelegate OnDeepLink;


        public static QuartersWebView OpenURL(string url) {
            
            GameObject webViewGO= new GameObject("QuartersWebView");
            QuartersWebView quartersWebView = webViewGO.AddComponent<QuartersWebView>();

            UniWebView webView = webViewGO.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, Screen.height / 4, Screen.width, Screen.height / 2);
            webView.Load(url);
            webView.Show();


            webView.OnPageStarted += (UniWebView view, string u) => {
                if (u.IsDeepLink()) {
                    //deep link opened
                    if (OnDeepLink != null) OnDeepLink(u);
                    webView.Hide();
                }

            };

            return quartersWebView;
        }




    }
}
