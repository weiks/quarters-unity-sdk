using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using CoinforgeSDK.UI;
using UnityEditor.SceneManagement;


namespace CoinforgeSDK.UI.Internal {
	[CustomEditor(typeof(UIStoryboard))]
	public class UIStoryboardEditor: Editor {


		[MenuItem("UI Toolkit/Create/Storyboard")]
		public static void CreateStoryboard() {

			GameObject newStoryboard = new GameObject("Storyboard");
			newStoryboard.AddComponent<UIStoryboard>();
			newStoryboard.AddComponent<EventSystem>();
			newStoryboard.AddComponent<StandaloneInputModule>();
			newStoryboard.AddComponent<TouchInputModule>();

			//select new storyboard
			Selection.activeGameObject = newStoryboard;
		}



		public override void OnInspectorGUI()
		{
			UIStoryboard storyboard = (UIStoryboard)target;

			EditorGUILayout.HelpBox("UIStoryboard is the main UI holder for the scene. Controlling UI resolution globally and holding reference to all views." +
				"You should have one storyoboard per scene. To create view enter view name then press Add view" , MessageType.Info, true);

			//resolution
			EditorGUILayout.LabelField("Resolution");
			//landscape
			EditorGUILayout.BeginHorizontal();
			//buttons with resolutions
			if (GUILayout.Button("iPad Air\nlandscape")) SetResolution("iPad Air landscape", new Vector2(2048, 1536));
			if (GUILayout.Button("iPhone 6\nlandscape")) SetResolution("iPhone 6 landscape", new Vector2(1334, 750));
			if (GUILayout.Button("iPhone 6 Plus\nlandscape")) SetResolution("iPhone 6 Plus landscape", new Vector2(2208, 1242));

			EditorGUILayout.EndHorizontal();
			//portrait
			EditorGUILayout.BeginHorizontal();
			//buttons with resolutions
			if (GUILayout.Button("iPad Air\nportrait")) SetResolution("iPad Air landscape", new Vector2(1536, 2048));
			if (GUILayout.Button("iPhone 6\nportrait")) SetResolution("iPhone 6 landscape", new Vector2(750, 1334));
			if (GUILayout.Button("iPhone 6 Plus\nportrait")) SetResolution("iPhone 6 Plus landscape", new Vector2(1242, 2208));


			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();


			//views
			EditorGUILayout.LabelField("Views");

			if (storyboard.Views.Count == 0) {
				EditorGUILayout.LabelField("No views present in the storyboard.\nAdd views by pressing <b>Add view</b> button", WarningStyle(), GUILayout.Height(30));
			}

		
			List<UIView> allViews = new List<UIView>();

			//remove manualy deleted views
			storyboard.Views.RemoveAll(view => view == null);
			allViews = new List<UIView>(storyboard.Views);


			foreach (UIView view in allViews) {
				DrawViewCell(view);
			}

			DrawAddViewCell();

			//initialView
			if (storyboard.initialView == null) {
				EditorGUILayout.HelpBox("Initial view is not assigned. Please assign initial view by dragging\nUIView game object into Initial view slot", MessageType.Warning, true);
			}

			storyboard.initialView = EditorGUILayout.ObjectField("Initial view", storyboard.initialView, typeof(UIView), true) as UIView;

		}




		public void SetResolution(string resolutionName, Vector2 newResolution) {
			UIStoryboard storyboard = (UIStoryboard)target;
			storyboard.SetResolution(newResolution);

			int width = Mathf.FloorToInt(newResolution.x);
			int height = Mathf.FloorToInt(newResolution.y);

			//gameview
			if (GameViewUtils.SizeExists(GameViewUtils.GetCurrentGroupType(), width, height)) {
				SetGameViewResolution(width, height);
			}
			else {
				//size doesnt exist yet, create it
				GameViewUtils.AddCustomSize(GameViewUtils.GameViewSizeType.FixedResolution, GameViewUtils.GetCurrentGroupType(), width, height, resolutionName);
				SetGameViewResolution(width, height);
			}



		}



		private void SetGameViewResolution(int width, int height) {
			int idx = GameViewUtils.FindSize(GameViewUtils.GetCurrentGroupType(), width, height);
			if (idx != -1) GameViewUtils.SetSize(idx);
		}





		public static Vector2 GetMainGameViewSize()
		{
			System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
			System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
			System.Object Res = GetSizeOfMainGameView.Invoke(null,null);
			return (Vector2)Res;
		}



		private GUIStyle WarningStyle() {

			GUIStyle style = new GUIStyle(EditorStyles.textField);
			style.normal.textColor = Color.yellow;
			style.richText = true;
			return style;
		}


	





		private string addViewName = "";

		private void DrawAddViewCell() {

			Rect cellRect = EditorGUILayout.BeginVertical();
			EditorGUI.DrawRect(cellRect, new Color(0.3f, 0.3f, 0.3f));
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			addViewName = EditorGUILayout.TextField("New view name:", addViewName);

			if (string.IsNullOrEmpty(addViewName)) GUI.enabled = false;
			if (GUILayout.Button("Add view")) {
				AddView(addViewName);
			}
			if (!GUI.enabled) GUI.enabled = true;

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();

		}



		private void AddView(string viewName) {
			UIStoryboard storyboard = (UIStoryboard)target;

			viewName = char.ToUpper(viewName[0]) + viewName.Substring(1);

			GameObject viewGO = new GameObject(viewName + "View");


			viewGO.transform.SetParent(storyboard.transform);

			//set view position
			viewGO.transform.localPosition = new Vector3(storyboard.Views.Count * 10f, 0);

			//camera
			Camera viewCamera = viewGO.AddComponent<Camera>();
			viewCamera.orthographic = true;
			viewCamera.orthographicSize = 1f;
			viewCamera.clearFlags = CameraClearFlags.Depth;
			viewCamera.depth = 50;
			viewCamera.useOcclusionCulling = false;
			viewCamera.nearClipPlane = -1f;
			viewCamera.farClipPlane = 1f;

			UIView view = viewGO.AddComponent<UIView>();


			//canvas
			GameObject canvasGO = new GameObject("UICanvas");
			canvasGO.transform.SetParent(viewGO.transform);
			Canvas canvas = canvasGO.AddComponent<Canvas>();
			canvas.renderMode = RenderMode.ScreenSpaceCamera;
			canvas.worldCamera = viewCamera;
			canvas.planeDistance = 0;


			//canvas scale
			CanvasScaler canvasScaler = canvasGO.AddComponent<CanvasScaler>();
			canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
			canvasScaler.referencePixelsPerUnit = 1f;
			canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
			canvasScaler.referenceResolution = storyboard.screenResolution;

			//viewContent
			GameObject viewContentGO = new GameObject("ViewContent");
			viewContentGO.transform.SetParent(canvasGO.transform, false);
			RectTransform viewContentRectTransform = viewContentGO.AddComponent<RectTransform>();

			viewContentRectTransform.anchorMin = Vector2.zero;
			viewContentRectTransform.anchorMax = Vector2.one;
			viewContentRectTransform.offsetMin = Vector2.zero;
			viewContentRectTransform.offsetMax = Vector2.one;
			viewContentRectTransform.localPosition = Vector3.zero;

			canvasGO.AddComponent<GraphicRaycaster>();
			canvasGO.AddComponent<CanvasGroup>();

			//add background
			GameObject backgroundGO = new GameObject("background");
			backgroundGO.transform.SetParent(viewContentGO.transform,false);
			Image background = backgroundGO.AddComponent<Image>();
			background.color = Color.grey;
			background.rectTransform.anchorMin = Vector2.zero;
			background.rectTransform.anchorMax = Vector2.one;
			background.rectTransform.pivot = new Vector2(0.5f, 0.5f);
			background.rectTransform.offsetMin = Vector2.zero;
			background.rectTransform.offsetMax = Vector2.zero;


			storyboard.Views.Add(view);


			addViewName = "";
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

		}


		private void RemoveView(UIView view) {
			UIStoryboard storyboard = (UIStoryboard)target;
			storyboard.Views.Remove(view);
			DestroyImmediate(view.gameObject);
			EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
		}






		private void DrawViewCell(UIView view) {

			Rect cellRect = EditorGUILayout.BeginVertical();
			EditorGUI.DrawRect(cellRect, new Color(0.15f, 0.15f, 0.15f));
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name", view.gameObject.name);
			if (GUILayout.Button("Remove")) {
				//display warrning dialog
				if (EditorUtility.DisplayDialog("Remove " + view.gameObject.name + "?", "Are you sure you want to remove this view? View content and code will be deleted",
					"Delete", "Cancel")) {
					RemoveView(view);
				}
			}
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
			EditorGUILayout.Space();

		}


	}


}