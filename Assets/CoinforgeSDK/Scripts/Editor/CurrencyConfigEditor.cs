using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CurrencyConfig))]
public class CurrencyConfigEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        CurrencyConfig config = (CurrencyConfig) target;

        
        GUIStyle errorStyle = new GUIStyle(EditorStyles.textField);
        errorStyle.normal.textColor = Color.red;

      
        EditorGUILayout.Space();
        foreach (char c in config.Code.ToCharArray()) {
            if (Char.IsUpper(c)) {
                EditorGUILayout.LabelField("Invalid currency code. Currency code must be lowercase", errorStyle);
            }
        }
        
        EditorGUILayout.Space();
        
        foreach (string productId in config.IAPProductIds) {
            foreach (char c in productId.ToCharArray()) {
                if (Char.IsUpper(c)) {
                    EditorGUILayout.LabelField("Invalid product ID. Product Ids must be lowercase", errorStyle);
                }
            }
        }
        

    }
    
    
    
}
