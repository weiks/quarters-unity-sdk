using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Runtime.InteropServices;
using ImaginationOverflow.UniversalDeepLinking;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.Serialization;

namespace QuartersSDK {
    public class QuartersWebView : MonoBehaviour {

        public delegate void OnDeepLinkDelegate(QuartersLink link);
        public static OnDeepLinkDelegate OnDeepLink;

        public delegate void OnCancelledDelegate();
        public static OnCancelledDelegate OnCancelled;
        
        
        private UniWebView webView;
        private bool renderEditorAuthorizationWindow = false;
        public UniWebView WebView {
            get {
                if (webView == null) {
                    webView = this.gameObject.AddComponent<UniWebView>();
                    webView.SetShowToolbar(false);
                    
                    if (Application.isEditor) {
                        float editorScaleFactor = 0.5f;
                        webView.Frame = new Rect(0, 0, Screen.width * editorScaleFactor, Screen.height * editorScaleFactor);
                    }
                    else {
                        //full screen
                        webView.Frame = new Rect(0, 0, Screen.width, Screen.height);
                    }

                    webView.OnPageStarted += WebViewOnOnPageStarted;
                }

                return webView;
            }
            set {
                webView = value;
            }
        }

        public void Init() {
            DeepLinkManager.Instance.LinkActivated += OnLinkActivated;
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

            if (Application.isEditor && Application.platform != RuntimePlatform.WindowsEditor) {
                linkType = LinkType.WebView;
            }
            
            if (linkType == LinkType.WebView) {
                WebView.Load(url);
                WebView.Show();
            }
            else {
                //external authorisation
                Application.OpenURL(url);
                if (Application.platform == RuntimePlatform.WindowsEditor) {
                    renderEditorAuthorizationWindow = true;
                }
            }
        }


        private Color backgroundColor = new Color(255f / 19f, 255f / 34f, 255f / 43f);
        public Rect windowRect = new Rect(0, Screen.height * 0.3f, Screen.width, Screen.height * 0.3f);
        private string editorAuthorizationUrl = string.Empty;
        void OnGUI() {
            if (!renderEditorAuthorizationWindow) return;
            
            GUI.color = backgroundColor;
            GUI.ModalWindow(0, windowRect, DrawEditorAuthWindow, "Quarters Editor Authorization");
        }
        

        void DrawEditorAuthWindow(int windowID) {
            
            EditorGUILayout.BeginVertical();
            GUI.color = Color.white;

            GUILayout.Label("1. Authorize user in the external browser window\n" +
                            "2. Then copy and paste browser url here\n" +
                            "3. Press Authorize button");
            editorAuthorizationUrl = GUILayout.TextArea(editorAuthorizationUrl, GUILayout.Height(100f));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Paste and Authorize")) {
                editorAuthorizationUrl = EditorGUIUtility.systemCopyBuffer;
                QuartersLink link = QuartersLink.Create(editorAuthorizationUrl);
                if (OnDeepLink != null) OnDeepLink(link);
                renderEditorAuthorizationWindow = false;
            }

            if (GUILayout.Button("Authorize")) {
                QuartersLink link = QuartersLink.Create(editorAuthorizationUrl);
                if (OnDeepLink != null) OnDeepLink(link);
                renderEditorAuthorizationWindow = false;
            }
            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }



    }
}
