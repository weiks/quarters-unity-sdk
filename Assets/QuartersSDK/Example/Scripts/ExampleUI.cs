using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Quarters {
	public class ExampleUI : MonoBehaviour {

		public List<Button> authorizedOnlyButtons = new List<Button>();


		void Awake() {
			authorizedOnlyButtons.ForEach(b => b.interactable = false);
		}


		private void RefreshButtons() {
			authorizedOnlyButtons.ForEach(b => b.interactable = Quarters.Instance.IsAuthorized);
		}





		public void ButtonAuthorizeTapped() {
			Quarters.Instance.Authorize(OnAuthorizationSuccess, OnAuthorizationFailed);
		}

		public void ButtonGetUserDetailsTapped() {
			Quarters.Instance.GetUserDetails(delegate(User user) {
				Debug.Log("User loaded");
			}, delegate (string error) {
				Debug.LogError("Cannot load the user details: " + error);
			});
		}





		public void OnAuthorizationSuccess() {
			Debug.Log("OnAuthorizationSuccess");

			RefreshButtons();
		}


		public void OnAuthorizationFailed(string error) {
			Debug.LogError("OnAuthorizationFailed: " + error);

			RefreshButtons();
		}


	}
}
