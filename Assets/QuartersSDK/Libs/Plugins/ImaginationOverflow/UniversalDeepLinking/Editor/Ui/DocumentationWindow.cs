using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Assets.Plugins.ImaginationOverflow.UniversalDeepLinking.Editor.Ui
{
    public class DocumentationWindow
    {
        [MenuItem("Window/ImaginationOverflow/Universal DeepLinking/Documentation", priority = 1)]

        public static void Show()
        {
            Application.OpenURL("https://universaldeeplinking.imaginationoverflow.com/docs");
        }
    }

    public class ReviewWindow
    {
        [MenuItem("Window/ImaginationOverflow/Universal DeepLinking/Review", priority = 9)]

        public static void Show()
        {
#if UDL_DLL_BUILD
            Application.OpenURL("https://assetstore.unity.com/packages/slug/125172");
#else
            Application.OpenURL("https://assetstore.unity.com/packages/slug/135654");
#endif
        }
    }
}
