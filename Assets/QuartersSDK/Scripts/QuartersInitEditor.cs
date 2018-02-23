using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Quarters;

[CustomEditor(typeof(QuartersInit))]
public class QuartersInitEditor : Editor {


    public override void OnInspectorGUI() {
        
        QuartersInit quartersInit = (QuartersInit)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Copy your App ID and App Key from your Quarters dashboard:");
        EditorGUILayout.Space();

        quartersInit.APP_ID = EditorGUILayout.TextField("App ID:", quartersInit.APP_ID);
        quartersInit.APP_KEY = EditorGUILayout.TextField("App key:", quartersInit.APP_KEY);
      

        //TODO Add missing fields inspector warrnings


    }


}

