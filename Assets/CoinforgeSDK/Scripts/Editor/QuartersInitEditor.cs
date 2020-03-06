using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using QuartersSDK;

[CustomEditor(typeof(QuartersInit))]
public class QuartersInitEditor : UnityEditor.Editor {


    public override void OnInspectorGUI() {
        
        QuartersInit quartersInit = (QuartersInit)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Copy your App ID and App Key from your Quarters dashboard:");
        EditorGUILayout.Space();

        base.DrawDefaultInspector();



    }


}

