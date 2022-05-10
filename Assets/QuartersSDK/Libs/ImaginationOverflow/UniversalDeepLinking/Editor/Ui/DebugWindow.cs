using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ImaginationOverflow.UniversalDeepLinking.Editor.Xcode;
using ImaginationOverflow.UniversalDeepLinking.Providers;
using UnityEditor;
using UnityEngine;


namespace ImaginationOverflow.UniversalDeepLinking.Editor.Ui
{
    public class DebugWindow : EditorWindow
    {
        private string _link;

        [MenuItem("Window/ImaginationOverflow/Universal DeepLinking/Debug", priority = 2)]

        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(DebugWindow), false, "UDL Debug").Show();
        }

        private void OnEnable()
        {
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("  ");
            EditorGUILayout.LabelField("Type an URL or URI that you which to debug.");
            EditorGUILayout.LabelField("  ");
            _link = EditorGUILayout.TextField("Debug Link", _link);


            if (GUILayout.Button("Debug"))
            {
                EditorLinkProvider.SimulateLink(_link);
            }

        }

    }
}

