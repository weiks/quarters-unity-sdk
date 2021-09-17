using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    [RequireComponent(typeof(Button))]
    public class UIShowShopButton : MonoBehaviour {
        
        
        private Button button {
            get { return this.GetComponent<Button>(); }
        }

        void Start() {
            //disable the existing callbacks
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(ButtonTapped);
        }
        
        
        public void ButtonTapped() {

            Quarters.Instance.BuyQuarters();
        }

    }
}
