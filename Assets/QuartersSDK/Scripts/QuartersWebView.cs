using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuartersSDK {
    public class QuartersWebView : MonoBehaviour {


        public static QuartersWebView OpenURL(string url) {
            
            GameObject webViewGO= new GameObject("QuartersWebView");
            QuartersWebView quartersWebView = webViewGO.AddComponent<QuartersWebView>();

            UniWebView webView = webViewGO.AddComponent<UniWebView>();
            webView.Frame = new Rect(0, Screen.height / 4, Screen.width, Screen.height / 2);
            webView.Load(url);
            webView.Show();

            webView.OnPageFinished += (UniWebView view, int statusCode, string u) => {

                Debug.Log(u);

            };

            return quartersWebView;

        }




    }
}
