using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QuartersSDK;
using DG.Tweening;


namespace QuartersSDK.UI {
    [RequireComponent(typeof(Text))]
    public class UICoinsCounter : MonoBehaviour {

        public Text CoinsCount {
            get {
                return GetComponent<Text>();
            }
        }
        
        private long currentCoins;
        
        private void OnEnable() {
            
            Quarters.OnUserLoaded += RefreshUser;
            Quarters.Instance.CurrentUser.OnBalanceUpdated += RefreshCoins;

            if (Quarters.Instance.CurrentUser != null) {
                RefreshUser(Quarters.Instance.CurrentUser);
            }
        }
        
        private void OnDisable() {
            Quarters.OnUserLoaded -= RefreshUser;
        }

        
        
        private void RefreshUser(User user) {
            RefreshCoins(Quarters.Instance.CurrentUser.Balance);
        }
        
       


        private void RefreshCoins(long availableCoins) {
                    
            if (currentCoins != availableCoins) {
                //coins updated
                CoinsCount.rectTransform.DOPunchScale(Vector3.one * 0.5f, 0.3f, 0, 0);
            }
            
            
            
            CoinsCount.text = String.Format("{0:n0}", availableCoins);;
            currentCoins = availableCoins;

        }
        
    }
}
