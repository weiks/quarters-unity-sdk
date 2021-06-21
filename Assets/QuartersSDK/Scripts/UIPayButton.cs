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


        private void Awake() {
            Assert.IsTrue(Price > 0, $"Incorrect Price: {Price} UIPayButton Price must be larger than zero");
        }


        void Start() {
            //disable the existing callbacks
            _buttonClickedEvent = button.onClick;
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(ButtonTapped);
        }


        public void ButtonTapped() {
            
            ModalView.instance.ShowActivity();
            
            TransferAPIRequest transferRequest = new TransferAPIRequest(Price, OnTransferSuccessful, OnTransferFailed);
            Quarters.Instance.CreateTransfer(transferRequest);
        }
        
        
        private void OnTransferSuccessful(string transactionHash) {
            ModalView.instance.HideActivity();
            
            SpendRewardView.Instance.Present(-Price);
            
            _buttonClickedEvent.Invoke();
        }


        
        private void OnTransferFailed(string error) {
            ModalView.instance.ShowAlert("Transaction error", error, new string[]{"OK"}, null);
        }
        
        
    }
}
