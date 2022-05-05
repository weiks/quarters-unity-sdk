using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK.UI;
using QuartersSDK;
using DG.Tweening;


namespace QuartersSDK.UI {
	public class AuthorizeView : UIView {

		public Image brandLogo;
		public Button LoginButton;

		public UISegue SegueToMainMenu;

		public bool AutomaticSignIn = false;


		private void Start() {
			if (AutomaticSignIn) {
				ButtonSignInClicked();
			}
		}

		public void ButtonSignInClicked() {
			LoginButton.interactable = false;
			QuartersInit.Instance.Init(OnInitComplete, OnInitError);
		}
		

		private void OnInitComplete() {
			SegueToMainMenu.Perform();
		}
		
		private void OnInitError(string error) {
			LoginButton.interactable = true;
		}
		

		

	}
}