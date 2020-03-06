using UnityEngine;
using UnityEditor;
using System.Collections;
using CoinforgeSDK.UI;

namespace CoinforgeSDK.UI.Internal {
	[CustomEditor(typeof(UISegue))]
	public class UISegueEditor : Editor {

		private Texture2D arrowTexture;

	
		public override void OnInspectorGUI (){
			
			UISegue segue = (UISegue)target;
			serializedObject.Update();

			EditorGUILayout.HelpBox("UISegue performing transition between UIViews", MessageType.Info, true);


			EditorGUILayout.BeginHorizontal();

			//source view
			Rect cellRect = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(150));
			EditorGUI.DrawRect(cellRect, ToolkitColor.shade);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Source view (locked)", GUILayout.Width(120));
			EditorGUILayout.ObjectField(segue.sourceView, typeof(UIView), true, GUILayout.Width(150));
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();


			//arrow
			//load arrow texture 
			if (arrowTexture == null) {
				string guid = AssetDatabase.FindAssets("UIToolkitIconSegueArrow")[0];
				string path = AssetDatabase.GUIDToAssetPath(guid);

				arrowTexture = (Texture2D)AssetDatabase.LoadMainAssetAtPath(path);
			}
			//draw arrow
			GUILayout.FlexibleSpace();
			GUILayout.Box(arrowTexture, GUIStyle.none);
			GUILayout.FlexibleSpace();



			//targetView
			Rect targetCellRect = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(150));
			EditorGUI.DrawRect(targetCellRect, ToolkitColor.shade);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Target view", GUILayout.Width(120));
			segue.targetView = EditorGUILayout.ObjectField(segue.targetView, typeof(UIView), true) as UIView;

			//rename segue automatically when assigning target view
			if (segue.targetView != null) {
				if (segue.name != "segueTo" + segue.targetView.name) segue.name = "segueTo" + segue.targetView.name;
			}

			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("transitionType"), new GUIContent("Transition type"), false);


			segue.animatedTransition = EditorGUILayout.ToggleLeft("Animated transition", segue.animatedTransition);

			if (segue.animatedTransition) {
				EditorGUILayout.PropertyField(serializedObject.FindProperty("animatedTransitionType"), new GUIContent("Animation type"), false);
			}


			serializedObject.ApplyModifiedProperties();
		}







	



	}
}
