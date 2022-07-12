using ImaginationOverflow.UniversalDeepLinking;
#if UNITY_EDITOR
    using UnityEditor; 
#endif

using UnityEngine;

namespace QuartersSDK {
    public class QuartersWebView : MonoBehaviour {
        public delegate void OnCancelledDelegate();

        public delegate void OnDeepLinkDelegate(QuartersLink link);

        public static OnDeepLinkDelegate OnDeepLink;

        public static OnCancelledDelegate OnCancelled;


        private readonly Color backgroundColor = new Color(255f / 19f, 255f / 34f, 255f / 43f);
        private string editorAuthorizationUrl = string.Empty;


        private bool renderEditorAuthorizationWindow;

        private UniWebView webView;

        public UniWebView WebView {
            get {
                if (webView == null) {
                    webView = gameObject.AddComponent<UniWebView>();
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
            set => webView = value;
        }

        public void Init() {
            DeepLinkManager.Instance.LinkActivated += OnLinkActivated;
        }


        private void OnDestroy() {
            DeepLinkManager.Instance.LinkActivated -= OnLinkActivated;
            if (webView != null) webView.OnPageStarted -= WebViewOnOnPageStarted;
        }


        //webview
        private void WebViewOnOnPageStarted(UniWebView webview, string url) {
            Log($"WebViewOnOnPageStarted: {url}");
            QuartersLink link = QuartersLink.Create(url);

            if (link.Uri.IsValidDeepLink()) {
                //deep link opened
                WebView.Hide();
                if (OnDeepLink != null) OnDeepLink(link);
            }
        }


        //external universal link
        private void OnLinkActivated(LinkActivation linkActivation) {
            Log($"ApplicationOnDeepLinkActivated: {linkActivation.Uri} is valid deep link: {linkActivation.Uri.IsValidDeepLink()}");
            QuartersLink link = QuartersLink.Create(linkActivation.Uri);

            if (link.Uri.IsValidDeepLink()) //deep link opened
                OnDeepLink?.Invoke(link);
        }


        public void OpenURL(string url, LinkType linkType) {
            if (Application.isEditor && Application.platform != RuntimePlatform.WindowsEditor) linkType = LinkType.WebView;

            if (linkType == LinkType.WebView) {
                WebView.Load(url);
                WebView.Show();
            }
            else if (linkType == LinkType.EditorExternal) {
                Application.OpenURL(url);
                renderEditorAuthorizationWindow = true;
            }
            else {
                //external authorisation
                Application.OpenURL(url);
            }
        }
        
#if UNITY_EDITOR
        private void OnGUI() {
            if (!renderEditorAuthorizationWindow) return;

            GUI.color = backgroundColor;
            GUI.ModalWindow(0, new Rect(0, Screen.height * 0.3f, Screen.width, Screen.height * 0.5f), DrawEditorAuthWindow, "Quarters Editor Authorization");
        }


        private void DrawEditorAuthWindow(int windowID) {
            EditorGUILayout.BeginVertical();
            GUI.color = Color.white;

            GUILayout.Label("1. Authorize user in the external browser window\n" +
                            "2. Then copy and paste browser url here\n" +
                            "3. Press Authorize button");
            editorAuthorizationUrl = GUILayout.TextArea(editorAuthorizationUrl, GUILayout.ExpandHeight(true));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel")) renderEditorAuthorizationWindow = false;

            if (GUILayout.Button("Paste and Authorize")) {
                editorAuthorizationUrl = EditorGUIUtility.systemCopyBuffer;
                AuthorizeEditor();
                renderEditorAuthorizationWindow = false;
            }

            if (GUILayout.Button("Authorize")) {
                AuthorizeEditor();
                renderEditorAuthorizationWindow = false;
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void AuthorizeEditor() {
            QuartersLink link = QuartersLink.Create(editorAuthorizationUrl);
            if (OnDeepLink != null) OnDeepLink(link);
        }
#endif
        
        private void Log(string message) {
            if (QuartersInit.Instance.ConsoleLogging == QuartersInit.LoggingType.Verbose) {
                Debug.Log(message);
            }
        }
        
        private void LogError(string message) {
            if (QuartersInit.Instance.ConsoleLogging == QuartersInit.LoggingType.Verbose) {
                Debug.LogError(message);
            }
        }
    }
}