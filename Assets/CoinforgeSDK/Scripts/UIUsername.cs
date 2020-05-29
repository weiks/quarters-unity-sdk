using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoinforgeSDK.UI {
    public class UIUsername : MonoBehaviour {

        public Text UsernameText {
            get {
                return GetComponent<Text>();
            }
        }
        
        private void OnEnable() {
            
            Coinforge.OnUserLoaded += RefreshUser;
            
            if (Coinforge.Instance.CurrentUser != null) {
                RefreshUser(Coinforge.Instance.CurrentUser);
            }
            
        }
        
        private void OnDisable() {
            Coinforge.OnUserLoaded -= RefreshUser;
        }
        
        
        private void RefreshUser(User user) {
            if (user.IsGuestUser) {
                UsernameText.text = "Guest";
            }
            else {
                UsernameText.text = user.displayName;
            }

        }
        
    }
}
