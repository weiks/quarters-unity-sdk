using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    [RequireComponent(typeof(Button))]
    public class UISideMenuButton : MonoBehaviour {
        

        private Button button {
            get { return this.GetComponent<Button>(); }
        }


        void Start() {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(ButtonTapped);
        }


        public void ButtonTapped() {
            FindObjectOfType<UISideMenu>().ShowMenu();
        }
        
  
        
    }
}
