using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    public class UIUsername : MonoBehaviour {

        public Text UsernameText {
            get {
                return GetComponent<Text>();
            }
        }
        
        private void OnEnable() {
            
            Quarters.OnUserLoaded += RefreshUser;
            
            if (Quarters.Instance.CurrentUser != null) {
                RefreshUser(Quarters.Instance.CurrentUser);
            }
            
        }
        
        private void OnDisable() {
            Quarters.OnUserLoaded -= RefreshUser;
        }
        
        
        private void RefreshUser(User user) {
            UsernameText.text = user.displayName;
        }
        
    }
}
