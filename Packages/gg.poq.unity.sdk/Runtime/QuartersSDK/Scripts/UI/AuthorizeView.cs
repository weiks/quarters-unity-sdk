using System;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    public class AuthorizeView : UIView {
        public Image brandLogo;
        public Button LoginButton;

        public UISegue SegueToMainMenu;

        public bool AutomaticSignIn = false;


        private void Start() {
            try
            {
                QuartersInit.Instance.Init(OnInitComplete, OnInitError);
            }
            catch (Exception ex)
            { 
                ModalView.instance.ShowAlert("Error", $"{ex.Message} \n {ex.StackTrace} ", new string[] { "OK" }, null);
            }
        }


        public void ButtonSignInClicked() {
            try 
            { 
                QuartersController.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
            }
            catch (Exception ex)
            {
                ModalView.instance.ShowAlert("Error", $"{ex.Message} \n {ex.StackTrace} ", new string[] { "OK" }, null);
            }
        }


        private void OnInitComplete() {
            try
            {
                if (AutomaticSignIn || QuartersController.Instance.IsAuthorized())
                {
                    QuartersController.Instance.SignInWithQuarters(OnSignInComplete, OnSignInError);
                }
            }
            catch (Exception ex)
            {
                ModalView.instance.ShowAlert("Error", $"{ex.Message} \n {ex.InnerException}", new string[] { "OK" }, null);
            }
        }

        private void OnSignInComplete() {
            try
            {
                SegueToMainMenu.Perform();
            }
            catch (Exception ex)
            { 
                ModalView.instance.ShowAlert("Error", $"{ex.Message} \n {ex.InnerException}", new string[] { "OK" }, null);
            }
        }


        private void OnInitError(string error) {
            Debug.LogError(error);
        }


        private void OnSignInError(string signInError) {
            ModalView.instance.ShowAlert("Error", $"{signInError} ", new string[] { "OK" }, null);
            Debug.Log(signInError);
        }
    }
}