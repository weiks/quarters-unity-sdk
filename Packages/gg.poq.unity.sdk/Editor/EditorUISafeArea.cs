using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UISafeArea))]
public class EditorUISafeArea : Editor {
	
	public override void OnInspectorGUI()
	{

		UISafeArea safeArea = (UISafeArea) target;

		
		safeArea.SimulateiPhoneX = EditorGUILayout.Toggle("Simulate iPhone X", safeArea.SimulateiPhoneX);
		if (safeArea.SimulateiPhoneX)
		{
			EditorGUILayout.LabelField("iPhone X simulation work only in Unity Editor. This value will be ignored when running outside Unity Editor");
			EditorGUILayout.Space();
		}
		

		EditorGUILayout.LabelField("Ignore margins:");
		safeArea.IgnoreTopMargin = EditorGUILayout.Toggle("Top", safeArea.IgnoreTopMargin);
		safeArea.IgnoreRightMargin = EditorGUILayout.Toggle("Right", safeArea.IgnoreRightMargin);
		safeArea.IgnoreBottomMargin = EditorGUILayout.Toggle("Bottom", safeArea.IgnoreBottomMargin);
		safeArea.IgnoreLeftMargin = EditorGUILayout.Toggle("Left", safeArea.IgnoreLeftMargin);
		
		safeArea.IgnoreOpositeNotchMargin = EditorGUILayout.Toggle("Oposite Notch", safeArea.IgnoreOpositeNotchMargin);
		
	
	}
}
