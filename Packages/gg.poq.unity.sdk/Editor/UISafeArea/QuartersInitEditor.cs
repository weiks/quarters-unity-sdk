using System.Collections;
using System.Collections.Generic;
using ImaginationOverflow.UniversalDeepLinking;
using ImaginationOverflow.UniversalDeepLinking.Storage;
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
        
        if (GUILayout.Button("Save")) {
            
            AppLinkingConfiguration config = new AppLinkingConfiguration();

            config.DisplayName = "Quarters SDK";
            config.DeepLinkingProtocols = new List<LinkInformation>();
            
            LinkInformation linkInformation = new LinkInformation();
            linkInformation.Scheme = "https";
            linkInformation.Host = $"{quartersInit.APP_UNIQUE_IDENTIFIER}.games.poq.gg";
            
            
            config.DeepLinkingProtocols.Add(linkInformation);
            
            ConfigurationStorage.Save(config);
            Debug.Log("Saved Quarters SDK Config");
            
        }



    }


}

