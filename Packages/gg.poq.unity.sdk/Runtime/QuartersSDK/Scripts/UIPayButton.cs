using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    [RequireComponent(typeof(Button))]
    public class UIPayButton : MonoBehaviour {
        
        [SerializeField] private int Price = 10;
        private Button.ButtonClickedEvent _buttonClickedEvent;

        private Button button {
            get { return this.GetComponent<Button>(); }
        }


        void Start() {
            //disable the existing callbacks
            _buttonClickedEvent = button.onClick;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(ButtonTapped);
        }


        public void ButtonTapped() {
            long price = -10;
            ModalView.instance.ShowActivity();

            QuartersController.Instance.Transaction((long) Price, "Example transaction", OnTransferSuccessful, OnTransferFailed);
        }


        private void OnTransferSuccessful() {
            ModalView.instance.HideActivity();

            SpendRewardView.Instance.Present(Price);
            _buttonClickedEvent.Invoke();
        }

        private void OnTransferFailed(string error) {
            if (error.Equals(Constants.QUARTERS_NOT_ENOUGH))
                ModalView.instance.ShowAlert("Quarters balance too low", "Your wallet does not have enough Quarters to make this purchase.", new string[] { "OK", Constants.BUY_QUARTERS_BUTTON }, ModalView.instance.alertButtonDelegate);
            else
                ModalView.instance.ShowAlert("Transaction error", error, new string[] { "OK" }, null);
        }
    }
}