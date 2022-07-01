using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using QuartersSDK.UI;
using QuartersSDK;


namespace QuartersSDK.UI {
    public class AuthorizeView : UIView {
        public Image brandLogo;
        public Button LoginButton;

        public UISegue SegueToMainMenu;

        public bool AutomaticSignIn = false;


        private void Start() {
            QuartersInit.Instance.Init(OnInitComplete, OnInitError);
        }


        public void ButtonSignInClicked() {
            QuartersController.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
        }


        private void OnInitComplete() {
            if (AutomaticSignIn || QuartersController.Instance.IsAuthorized) {
                QuartersController.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
            }
        }

        private void OnSignInComplete() {
            SegueToMainMenu.Perform();
        }


        private void OnInitError(string error) {
            Debug.LogError(error);
        }


        private void OnSignInError(string signInError) {
            Debug.Log(signInError);
        }
    }
}