using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    [RequireComponent(typeof(Button))]
    public class UIBuyQuartersButton : MonoBehaviour {
        
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
            
            ModalView.instance.ShowActivity();
            
            Quarters.Instance.BuyQuarters();
            
            
        }
        
        
        private void OnTransferSuccessful() {

            _buttonClickedEvent.Invoke();
        }


        
        private void OnTransferFailed(string error) {
            ModalView.instance.ShowAlert("Transaction error", error, new string[]{"OK"}, null);
        }
        
        
    }
}
