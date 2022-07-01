using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    [RequireComponent(typeof(Button))]
    public class UISignOutButton : MonoBehaviour {
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
            QuartersController.Instance.Deauthorize();
        }
    }
}