using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuartersSDK.UI {
    public class UIUsername : MonoBehaviour {
        public Text UsernameText {
            get { return GetComponent<Text>(); }
        }

        private void OnEnable() {
            QuartersController.OnUserLoaded += RefreshUser;

            if (QuartersController.Instance.CurrentUser != null) {
                RefreshUser(QuartersController.Instance.CurrentUser);
            }
        }

        private void OnDisable() {
            QuartersController.OnUserLoaded -= RefreshUser;
        }


        private void RefreshUser(User user) {
            UsernameText.text = user.GamerTag;
        }
    }
}