using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace QuartersSDK.UI {
	public class UIView : MonoBehaviour {

		private Canvas[] canvases {
			get {
				return this.GetComponentsInChildren<Canvas>();
			}
		}
		[HideInInspector] public CanvasScaler canvasScaler {
			get {
				return canvases[0].GetComponent<CanvasScaler>();
			}
		}
		[HideInInspector] public Camera camera {
			get {
				return this.GetComponent<Camera>();
			}
		}
		[HideInInspector] public RectTransform viewContent {
			get {
				return (RectTransform)canvasScaler.transform.Find("ViewContent");
			}
		}

  

		public bool IsVisible {
			get { return camera.enabled && canvases[0].enabled; }
		}

		public bool InputActive {
			set {
				canvasScaler.gameObject.GetComponent<GraphicRaycaster>().enabled = value;
			}
			get {
				return canvasScaler.gameObject.GetComponent<GraphicRaycaster>().enabled;
			}
		}

		private CanvasGroup canvasGroup {
			get {
				return canvasScaler.gameObject.GetComponent<CanvasGroup>();
			}
		}


		public virtual void Awake() {
			
		}

		public virtual void ViewWillAppear(UIView sourceView = null) {
			
		}

		public virtual void ViewAppeared() {
			InputActive = true;
		}

		public virtual void ViewWillDissappear() {
			InputActive = false;
		}

		public virtual void ViewDisappeared() {

		}
			


		public void SetVisible(bool isVisible) {
			camera.enabled = isVisible;
			foreach (Canvas canvas in canvases) {
				canvas.enabled = isVisible;
			}

			foreach (Graphic graphic in this.transform.GetComponentsInChildren<Graphic>()) {
				graphic.SetAllDirty();
			}
		}


		public void SetResolution(Vector2 newResolution) {
			canvasScaler.referenceResolution = newResolution;

			//refresh camera
			bool cameraWasDisabled = camera.enabled;
			camera.enabled = !cameraWasDisabled;
			camera.enabled = cameraWasDisabled;
		}


		public void SetAlpha(float alpha) {
			canvasGroup.alpha = alpha;	
		}


		#region Gizmos


		#if UNITY_EDITOR
		void OnDrawGizmos() {
			DrawOutline(Color.yellow);
			Handles.Label(camera.ViewportToWorldPoint(Vector3.zero), gameObject.name.Replace("View", ""));
		}

		void OnDrawGizmosSelected () {
			DrawOutline(Color.green);
		}

		void DrawOutline(Color color) {
			//draw view outline
			DrawLine(Vector2.zero, new Vector2(0, 1f), color);
			DrawLine(new Vector2(0, 1f), new Vector2(1f, 1f), color);
			DrawLine(new Vector2(1f, 1f), new Vector2(1f, 0), color);
			DrawLine(new Vector2(1f, 0), new Vector2(0, 0), color);
		}
		
		void DrawLine(Vector2 screenPosFrom, Vector2 screenPosTo, Color color) {

			Vector3 worldPosFrom = camera.ViewportToWorldPoint(new Vector3(screenPosFrom.x, screenPosFrom.y, camera.farClipPlane - camera.nearClipPlane));
			Vector3 worldPosTo = camera.ViewportToWorldPoint(new Vector3(screenPosTo.x, screenPosTo.y, camera.farClipPlane - camera.nearClipPlane));
			Debug.DrawLine(worldPosFrom, worldPosTo, color);
		}

		#endif


		#endregion



		
	}
}
