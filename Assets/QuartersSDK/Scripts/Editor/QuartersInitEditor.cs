using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using QuartersSDK;

[CustomEditor(typeof(QuartersInit))]
public class QuartersInitEditor : UnityEditor.Editor {


    public override void OnInspectorGUI() {
        
        QuartersInit quartersInit = (QuartersInit)target;

        EditorGUILayout.LabelField($"Quarters Unity SDK - Version {QuartersInit.SDK_VERSION}");
        if (GUILayout.Button("Open Dashboard")) {
            Application.OpenURL(quartersInit.DASHBOARD_URL);
        }
        EditorGUILayout.Space();
        base.DrawDefaultInspector();



    }


}

