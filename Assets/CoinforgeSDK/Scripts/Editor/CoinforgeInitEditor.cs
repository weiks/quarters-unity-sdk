using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CoinforgeSDK;

[CustomEditor(typeof(CoinforgeInit))]
public class CoinforgeInitEditor : UnityEditor.Editor {


    public override void OnInspectorGUI() {
        
        CoinforgeInit coinforgeInit = (CoinforgeInit)target;

        EditorGUILayout.LabelField($"Coinforge Unity SDK - Version {CoinforgeInit.SDK_VERSION}");
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Copy your App ID and App Key from your Coinforge dashboard:");
        EditorGUILayout.Space();

        base.DrawDefaultInspector();



    }


}

