using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;


namespace CoinforgeSDK.UI {
	[System.Serializable]
	public class UIStoryboard : MonoBehaviour {

		[SerializeField]
		public UIView initialView;
		public Vector2 screenResolution = new Vector2(2048, 1536);
		public List<UIView> Views {
			get {
				return new List<UIView>(this.GetComponentsInChildren<UIView>());
			}	
		}


        public bool isFirstScene = false;

        public AudioClip clickFX;



		public void Start() {
			//initial view presentation and hiding other view
			foreach (UIView view in Views) {
				view.ViewWillDissappear();
				view.SetVisible(false);
				view.ViewDisappeared();

			}

			initialView.ViewWillAppear();
			initialView.SetVisible(true);
			initialView.ViewAppeared();



			StartCoroutine(RefreshEventSystem());
		}


		//workaround the locked input Unity bug after reloading scene
		private IEnumerator RefreshEventSystem() {

			EventSystem eventSystem = this.GetComponent<EventSystem>();

			eventSystem.enabled = false;

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			eventSystem.enabled = true;

			eventSystem.UpdateModules();


		}


		public void SetResolution(Vector2 newResolution) {
			screenResolution = newResolution;
			foreach (UIView view in Views) view.SetResolution(newResolution);

		}



	

	}
}
