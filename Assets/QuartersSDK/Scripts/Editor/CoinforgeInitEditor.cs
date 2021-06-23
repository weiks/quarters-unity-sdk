using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using QuartersSDK;

[CustomEditor(typeof(QuartersInit))]
public class CoinforgeInitEditor : UnityEditor.Editor {


    public override void OnInspectorGUI() {
        
        QuartersInit quartersInit = (QuartersInit)target;

        EditorGUILayout.LabelField($"Coinforge Unity SDK - Version {QuartersInit.SDK_VERSION}");
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Copy your App ID and App Key from your Coinforge dashboard:");
        EditorGUILayout.Space();

        base.DrawDefaultInspector();



    }


}

