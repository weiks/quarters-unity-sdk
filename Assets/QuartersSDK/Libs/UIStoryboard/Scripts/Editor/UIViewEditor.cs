using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK.UI;
using UnityEditor.SceneManagement;


namespace QuartersSDK.UI.Internal {
	[CustomEditor(typeof(UIView), true)]
	public class UIViewEditor: Editor {

		private bool isCreatingClass = false;

		public override void OnInspectorGUI()
		{
			if (isCreatingClass) {
				CreatingClass();
				return;
			}

			UIView view = (UIView)target;

			EditorGUILayout.HelpBox("UIView is the controller for the view. To add custom logic to the view use Create subclass option", MessageType.Info, true);

			EditorGUILayout.LabelField("Tools");

			Rect basicRect = EditorGUILayout.BeginVertical();
			EditorGUI.DrawRect(basicRect, ToolkitColor.shade);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Basic");

			EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button("Add segue")) {
					GameObject segueGO = new GameObject("segue");
					segueGO.transform.SetParent(view.transform);
					segueGO.transform.localPosition = Vector3.zero;
					UISegue segue = segueGO.AddComponent<UISegue>();

					Selection.activeGameObject = segue.gameObject;
				}

			if (GUILayout.Button("Show Preview")) {
				SetPreviewVisibility(true);
			}

			if (GUILayout.Button("Hide Preview")) {
				SetPreviewVisibility(false);
			}


			EditorGUILayout.EndHorizontal();


			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();


			//only show create subclass button if we are showing baseclass
			if (!this.ContainsComponentWithName(view.name)) {

				Rect advancedRect = EditorGUILayout.BeginVertical();
				EditorGUI.DrawRect(advancedRect, ToolkitColor.shade);

					EditorGUILayout.Space();
					EditorGUILayout.LabelField("Advanced");

					if (GUILayout.Button("Create subclass")) {
						Utilities.CreateViewClass(view.name);
						isCreatingClass = true;
					}

					
				EditorGUILayout.Space();
				EditorGUILayout.EndVertical();
			}

			base.DrawDefaultInspector();
	
		}





		void CreatingClass() {
			UIView view = (UIView)target;

			if (EditorApplication.isCompiling) {
				EditorUtility.DisplayProgressBar("Creating view class", "Waiting for compiler to finish...", 0.5f);
				//locking selection
				Selection.activeGameObject = view.gameObject;
			}
			else {

				EditorUtility.ClearProgressBar();
				isCreatingClass = false;
				//add component
				UnityEngineInternal.APIUpdaterRuntimeServices.AddComponent(view.gameObject, "", view.name);

				//assign
				foreach(Component component in view.GetComponents(typeof(Component))) {
					if (component.GetType().Name == view.name) {
						//reassign values from UIView to subclass
						UIView oldView = view;
						UIView subClassView = (UIView)component;


						//reconnect all target view segues
						foreach(UISegue segue in FindObjectsOfType<UISegue>()) {
							if (segue.targetView == oldView) {
								segue.targetView = subClassView;
							}
						}




						//remove baseclass component
						DestroyImmediate(oldView);
						Repaint();
						Debug.Log("Subclass: " + component.GetType() + " created successfully");
					}
				}


			}
		}


		private bool ContainsComponentWithName(string componentName) {
			UIView view = (UIView)target;

			foreach(Component component in view.GetComponents(typeof(Component))) {
				if (component.GetType().Name == componentName) {
					return true;
				}
			}

			//not found
			return false;
		}





		private void SetPreviewVisibility(bool isVisible) {
			UIView view = (UIView)target;

			view.camera.enabled = isVisible;
			Canvas[] canvases = view.gameObject.GetComponentsInChildren<Canvas>();
			foreach (Canvas canvas in canvases) canvas.enabled = isVisible;


		}


	}

}