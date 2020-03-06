using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace CoinforgeSDK.UI {
	public class UISegue : MonoBehaviour {

		public bool animatedTransition = false;
		public enum AnimatedTransitionType {
			pushLeft,
			pushRight,
			fade
		}
		public AnimatedTransitionType animatedTransitionType = AnimatedTransitionType.pushLeft;
		private float transitionTime = 0.35f;


		public enum TransitionType {
			swap,
			presentOnTop,
			dismiss
		}
		public TransitionType transitionType = TransitionType.swap;

		public UIView overrideSourceView;
		
		public UIView sourceView {
			get {
				if (overrideSourceView == null) {
					return this.transform.GetComponentInParent<UIView>();
				}
				else return overrideSourceView;
			}	
		}
			
		[SerializeField]
		public UIView targetView;


		protected void Awake() {
			if (transitionType != TransitionType.dismiss) {
//				if (targetView == null && transitionType == TransitionType.swap) Debug.LogError(this.gameObject.name + " target view is unassigned", this.gameObject);
			}
		}



		public void Perform() {
			Invoke("PerformFromMainThread",0);

		}


		private bool sourceViewDismiss {
			get { return (transitionType == TransitionType.swap) || (transitionType == TransitionType.dismiss); }
		} 

		private bool targetViewAppear {
			get { return (transitionType == TransitionType.swap) || (transitionType == TransitionType.presentOnTop); }
		} 

		private void PerformFromMainThread() {

            Debug.Log("UISegue: " + this.gameObject.name, this.gameObject);

			//make sure new view is presented on top
			if (transitionType == TransitionType.presentOnTop) {
				targetView.camera.depth = sourceView.camera.depth + 1;
			}
				

			if (!animatedTransition) {

				if (sourceViewDismiss) sourceView.ViewWillDissappear();
				if (targetViewAppear) targetView.ViewWillAppear(sourceView);
				if (sourceViewDismiss) sourceView.SetVisible(false);
                if (targetViewAppear) targetView.SetAlpha(1f);
				if (targetViewAppear) targetView.SetVisible(true);
				TransitionCompleted();
			}
			else {
				//animated transitions
				if (sourceViewDismiss) sourceView.ViewWillDissappear();
				if (targetViewAppear) targetView.ViewWillAppear(sourceView);

				//animated code here
				if (animatedTransitionType == AnimatedTransitionType.pushLeft) {


					//sourceView
					if (sourceViewDismiss) iTween.MoveTo(sourceView.viewContent.gameObject, iTween.Hash("x", -sourceView.viewContent.rect.width,
						"time", transitionTime,
						"islocal", true, "oncompletetarget", this.gameObject, "oncomplete", "HideSource"));


					//targetView
                    if (targetViewAppear) targetView.viewContent.localPosition = new Vector3(targetView.viewContent.rect.width, targetView.viewContent.localPosition.y, 0);
					if (targetViewAppear) targetView.SetAlpha(1f);
					if (targetViewAppear) targetView.SetVisible(true);

					iTween.MoveTo(targetView.viewContent.gameObject, iTween.Hash("x", 0,
						"time", transitionTime,
						"islocal", true, "oncompletetarget", this.gameObject, "oncomplete", "TransitionCompleted"));

				}
				else if (animatedTransitionType == AnimatedTransitionType.pushRight) {

					//sourceView
					if (sourceViewDismiss) iTween.MoveTo(sourceView.viewContent.gameObject, iTween.Hash("x", sourceView.viewContent.rect.width,
						"time", transitionTime,
						"islocal", true, "oncompletetarget", this.gameObject, "oncomplete", "HideSource"));


					//targetView
                    if (targetViewAppear) targetView.viewContent.localPosition = new Vector3(-targetView.viewContent.rect.width, targetView.viewContent.localPosition.y, 0);
					if (targetViewAppear) targetView.SetAlpha(1f);
					if (targetViewAppear) targetView.SetVisible(true);

					if (targetViewAppear) iTween.MoveTo(targetView.viewContent.gameObject, iTween.Hash("x", 0,
						"time", transitionTime,
						"islocal", true, "oncompletetarget", this.gameObject, "oncomplete", "TransitionCompleted"));

				}
				else if (animatedTransitionType == AnimatedTransitionType.fade) {

					StartCoroutine(PerformFade());
				}
			}
		}









		IEnumerator PerformFade() {


			//targetView
			if (targetViewAppear) {
				targetView.SetAlpha(0);
				targetView.SetVisible(true);
                targetView.viewContent.localPosition = new Vector3(0, targetView.viewContent.localPosition.y, 0);
				iTween.ValueTo(targetView.gameObject, iTween.Hash("from", 0, "to", 1f, "time", transitionTime/2f, "onupdate", "SetAlpha"));
				yield return new WaitForSeconds(transitionTime/2f);
			}

			if (sourceViewDismiss) {
				iTween.ValueTo(sourceView.gameObject, iTween.Hash("from", 1f, "to", 0, "time", transitionTime/2f, "onupdate", "SetAlpha"));
				yield return new WaitForSeconds(transitionTime/2f);
				sourceView.SetVisible(false);
			}

			TransitionCompleted();
		}






		void HideSource() {
			sourceView.SetVisible(false);
		}



		void TransitionCompleted() {
			if (sourceViewDismiss) sourceView.ViewDisappeared();
            if (sourceViewDismiss) sourceView.viewContent.localPosition = new Vector3(0, sourceView.viewContent.localPosition.y, 0);
			if (targetViewAppear) targetView.ViewAppeared();
		}







		void OnDrawGizmosSelected() {
			
			Gizmos.color = Color.yellow;
			if (targetView != null) {
				//draw line
				Gizmos.DrawLine(sourceView.transform.position, targetView.transform.position);
			}

		}



	}
}
